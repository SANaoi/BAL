using System;
using System.Collections;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.UI;
using static Aki.Scripts.UI.DataSparkAISTT;

namespace Aki.Scripts.UI
{
    public class SparkAISTT : STT
    {
        private ClientWebSocket webSocket;
        private CancellationTokenSource connectionCTS = new CancellationTokenSource();
        private CancellationTokenSource receiveCTS = new CancellationTokenSource();
        private string apiKey = "98989f21fb7430bf845fc191784b70fd";
        private string apiSecret = "MjNhM2FhYzY0OWQ5ZGM5MjJkZjdkNjFh";
        private string appId = "064b0378";

        private string wsUrl = "wss://iat-api.xfyun.cn/v2/iat";
        private string authorization = "";
        private int sampleRate = 16000; // 采样率
        private int frameSize = 12800;
        private bool isFinalResultReceived;

        private StringBuilder realtimeText = new StringBuilder();
        public Text text; // 用于显示实时文本

        void Update()
        {
            Debug.Log($"WebSocket状态: {webSocket?.State}");
            text.text = realtimeText.ToString(); // 更新UI文本
        }
        public void OnStart()
        {
            SpeechToText(Microphone.Start(null, true, 48, sampleRate), (result) => { });
        }

        public override void SpeechToText(AudioClip _clip, Action<string> _callback)
        {
            base.SpeechToText(_clip, _callback);
            StartCoroutine(SendAudioData(_clip, _callback));
        }

        IEnumerator SendAudioData(AudioClip _clip, Action<string> _callback)
        {
            // isRecording = true;
            yield return null;
            _ = OnStartRecoding(_clip);
            _callback?.Invoke(new string("语音识别中..."));
        }

        async Task OnStartRecoding(AudioClip audioClip)
        {
            try
            {
                DateTime timestamp = DateTime.Now;
                string signedUrl = GetAuthUrl
                (
                    apiKey,
                    apiSecret,
                    wsUrl,
                    timestamp
                );
                Debug.Log($"WebSocket连接地址: {signedUrl}");
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(signedUrl), connectionCTS.Token);

                
                if (webSocket.State == WebSocketState.Open)
                {
                    _ = ReceiveMessage(); // 启动接收任务
                    StartCoroutine(SendAudioFrames(audioClip));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket连接失败: {ex.Message}");
            }
        }

        #region 生成请求地址
        string GenerateSignature(string apiKey, string apiSecret, string url, DateTime date)
        {
            string host = new Uri(url).Host;  // 从URL提取host
            string requestLine = $"GET {new Uri(url).PathAndQuery} HTTP/1.1"; // 构建 request-line
            string dateStr = date.ToString("r");
            string signatureOrigin = $"host: {host}\ndate: {dateStr}\n{requestLine}";
            byte[] signatureBytes;
            using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(apiSecret)))
            {
                signatureBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(signatureOrigin));
            }
            string signature = Convert.ToBase64String(signatureBytes);

            // 构建authorization_origin
            string authorizationOrigin = $"api_key=\"{apiKey}\", algorithm=\"hmac-sha256\", headers=\"host date request-line\", signature=\"{signature}\"";
            byte[] authorizationBytes = Encoding.ASCII.GetBytes(authorizationOrigin);
            return authorization = Convert.ToBase64String(authorizationBytes);
        }

        string GetAuthUrl
        (
            string apiKey,
            string apiSecret,
            string url,
            DateTime timesTemp
        )
        {
            string host = new Uri(url).Host;
            string dateStr = timesTemp.ToString("r");
            // URL编码参数
            string encodedDate = HttpUtility.UrlEncode(dateStr);
            string encodedHost = HttpUtility.UrlEncode(host);
            // generate signature
            authorization = GenerateSignature(apiKey, apiSecret, url, timesTemp);

            return $"{url}?authorization={authorization}&date={encodedDate}&host={encodedHost}";
        }
        #endregion

        #region 启动音频采集和发送
        IEnumerator SendAudioFrames(AudioClip audioClip)
        {
            int samplesPerFrame = frameSize / 2; // 假设 frameSize=1280字节 → 640样本
            float[] audioData = new float[samplesPerFrame];

            int readPos = 0;
            while (webSocket.State == WebSocketState.Open)
            {
                int currentPos = Microphone.GetPosition(null);
                if (currentPos < readPos)
                {
                    currentPos += audioClip.samples;
                }
                int availableSamples = currentPos - readPos;

                // 处理所有可用帧
                while (availableSamples >= samplesPerFrame)
                {
                    int readIndex = readPos % audioClip.samples;
                    audioClip.GetData(audioData, readIndex);

                    byte[] pcmBytes = ConvertAudioToPCM(audioData);
                    bool isFirst = (readPos == 0);
                    bool isLast = false;
                    _ = SendAudioFrame(pcmBytes, isFirst, isLast);

                    readPos = (readPos + frameSize / 2) % audioClip.samples;
                    availableSamples -= samplesPerFrame;
                }

                yield return new WaitForSeconds(0.04f);
            }
        }
        public byte[] ConvertClipToBytes(AudioClip audioClip)
        {
            float[] samples = new float[audioClip.samples];

            audioClip.GetData(samples, 0);

            short[] intData = new short[samples.Length];

            byte[] bytesData = new byte[samples.Length * 2];

            int rescaleFactor = 32767;

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }
        byte[] ConvertAudioToPCM(float[] audioData)
        {
            byte[] pcmBytes = new byte[frameSize];

            int byteIndex = 0;

            foreach (float sample in audioData)
            {
                // 将浮点数转换为16位PCM格式
                short pcmValue = (short)(sample * short.MaxValue);
                pcmBytes[byteIndex++] = (byte)(pcmValue & 0x00FF); // 低字节
                pcmBytes[byteIndex++] = (byte)((pcmValue >> 8) & 0x00FF); // 高字节
            }
            return pcmBytes;
        }

        string BuildIatRequestFrame(byte[] pcmdata, bool isFirst, bool isLast)
        {
            IatRequest request = new IatRequest
            {
                common = new IatRequest.Common { app_id = appId },
                business = new IatRequest.Business(),
                data = new IatRequest.Data
                {
                    status = isFirst ? 0 : (isLast ? 2 : 1),
                    audio = Convert.ToBase64String(pcmdata)
                }
            };
            return JsonUtility.ToJson(request);
        }

        async Task SendAudioFrame(byte[] pcmData, bool isFirst, bool isLast)
        {
            string jsonRequest = BuildIatRequestFrame(pcmData, isFirst, isLast);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonRequest);

            try
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(jsonBytes),
                    WebSocketMessageType.Text,
                    true,
                    connectionCTS.Token
                );
            }
            catch (WebSocketException ex)
            {
                Debug.LogError($"发送失败: {ex.Message}");
            }
        }
        #endregion

        #region 接收数据
        async Task ReceiveMessage()
        {
            try
            {
                byte[] buffer = new byte[4096];
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        receiveCTS.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string jsonResponse = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessResponse(jsonResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"接收失败: {ex}");
            }
        }

        void ProcessResponse(string json)
        {
            try
            {
                SparkResponse response = JsonUtility.FromJson<SparkResponse>(json);
                if (response.code != 0)
                {
                    Debug.LogError($"识别失败: {response.code} - {response.message}");
                    return;
                }

                if (response.data?.status == 1)
                {
                    realtimeText.Append(GetWords(response));
                    Debug.Log($"中间结果: {realtimeText}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"处理响应异常: {ex}");
            }
        }
        async void CloseWebSocket()
        {
            try
            {
                if (webSocket != null)
                {
                    if (webSocket.State == WebSocketState.Open)
                    {
                        // 循环结束后发送尾帧
                        _ = SendAudioFrame(new byte[0], isFirst: false, isLast: true);

                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "User requested close",
                            CancellationToken.None
                        );
                    }
                    connectionCTS.Cancel();
                    receiveCTS.Cancel();
                    webSocket?.Dispose();
                    webSocket = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"关闭WebSocket失败: {ex.Message}");
            }
        }
        public void StopRecoding()
        {
            text.text = "";
            // 延迟等待最终结果
            StartCoroutine(WaitForFinalResult());
            Microphone.End(null); // 停止麦克风录音
        }
        IEnumerator WaitForFinalResult()
        {
            yield return new WaitForSeconds(1f); // 等待1秒以确保接收完数据
            CloseWebSocket();
        }

        /// <summary>
        /// 获取识别到的文本
        /// </summary>
        /// <param name="_responseData"></param>
        /// <returns></returns>
        private string GetWords(SparkResponse _responseData)
        {
            string stringBuilder = "";
            foreach (var item in _responseData.data.result.ws)
            {
                foreach (var _cw in item.cw)
                {
                    stringBuilder += _cw.w;
                }
            }
            return stringBuilder;
        }
        #endregion
    }
}
