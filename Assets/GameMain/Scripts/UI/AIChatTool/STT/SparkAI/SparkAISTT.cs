using System;
using System.Collections;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Aki.AI.Demo;
using UnityEngine;
using static Aki.Scripts.UI.DataSparkAISTT;

namespace Aki.Scripts.UI
{
    public class SparkAISTT : STT
    {
        private ClientWebSocket webSocket;
        // private CancellationToken cancellationToken;
        private CancellationTokenSource timeoutCTS = new CancellationTokenSource();
        private string apiKey = "98989f21fb7430bf845fc191784b70fd";
        private string apiSecret = "MjNhM2FhYzY0OWQ5ZGM5MjJkZjdkNjFh";
        private string appId = "064b0378";

        private string wsUrl = "wss://iat-api.xfyun.cn/v2/iat";
        private string authorization = "";

        private int frameSeq = 1; // 帧序列号（需递增）
        private bool isRecording = false;
        private int sampleRate = 16000; // 采样率
        private int frameSize = 1280; // 每帧大小

        private bool isFinalResultReceived;
        private StringBuilder realtimeText = new StringBuilder();

        void Update()
        {
            Debug.Log($"WebSocket状态: {webSocket?.State}");
        }
        public void OnStart()
        {
            SpeechToText(Microphone.Start(null, true, 5, sampleRate), (result) => { });
        }

        public override void SpeechToText(AudioClip _clip, Action<string> _callback)
        {
            base.SpeechToText(_clip, _callback);
            StartCoroutine(SendAudioData(_clip, _callback));
        }

        IEnumerator SendAudioData(AudioClip _clip, Action<string> _callback)
        {
            isRecording = true;
            yield return null;
            _ = OnStartRecoding(_clip);
            _callback?.Invoke(new string("语音识别中..."));
        }

        async Task OnStartRecoding(AudioClip audioClip)
        {
            try
            {
                DateTime timestamp = DateTime.Now;
                string signedUrl = BuildWebSocketUrl
                (
                    apiKey,
                    apiSecret,
                    wsUrl,
                    timestamp
                );
                Debug.Log($"WebSocket连接地址: {signedUrl}");
                webSocket = new ClientWebSocket();
                // cancellationToken = new CancellationToken();
                // 设置超时（10秒）
                // CancellationTokenSource timeoutCTS = new CancellationTokenSource(10000);
                await webSocket.ConnectAsync(new Uri(signedUrl), timeoutCTS.Token);

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

        string BuildWebSocketUrl
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
            float[] audioData = new float[frameSize / 2]; // 每帧16000Hz*16bit=1280字节

            while (Microphone.GetPosition(null) <= 0)
            {
                yield return null; // 等待麦克风初始化
            }

            int readPos = 0;
            while (webSocket.State == WebSocketState.Open)
            {

                int currentPos = Microphone.GetPosition(null);
                if (currentPos < readPos)
                {
                    currentPos += audioClip.samples; // 处理循环录音
                }
                int availableSamples = currentPos - readPos;
                if (availableSamples >= frameSize / 2)
                {
                    // 读取正确的采样点数
                    audioClip.GetData(audioData, readPos % audioClip.samples);

                    // 转换为PCM字节
                    byte[] pcmBytes = ConvertAudioToPCM(audioData);

                    bool isFirst = readPos == 0;
                    bool isLast = false;

                    _ = SendAudioFrame(pcmBytes, isFirst, isLast);

                    readPos += audioData.Length;
                }
                yield return new WaitForSeconds(0.04f); // 每40ms发送一次数据
            }

            // 循环结束后发送尾帧
            _ = SendAudioFrame(new byte[0], isFirst: false, isLast: true);
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
                header = new IatRequest.Header
                {
                    app_id = appId,
                    status = isFirst ? 0 : (isLast ? 2 : 1),
                },
                parameter = new IatRequest.Parameter // 必须始终存在
                {
                    iat = new IatRequest.Parameter.IatParams
                    {
                        result = new IatRequest.Parameter.IatParams.ResultParams()
                    }
                },
                payload = new IatRequest.Payload
                {
                    audio = new IatRequest.Payload.AudioData
                    {
                        seq = frameSeq++,
                        status = isFirst ? 0 : (isLast ? 2 : 1),
                        audio = Convert.ToBase64String(pcmdata)
                    }
                }
            };
            // 移除未使用的字段（中间帧无parameter）
            if (!isFirst) request.parameter = null; // 非首帧置空
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
                    timeoutCTS.Token
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
                var memoryStream = new MemoryStream(); // 新增内存流

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None // 建议使用独立Token
                    );
                    Debug.Log($"接收数据: {result.Count}字节, 结束标志: {result.EndOfMessage}");
                    memoryStream.Write(buffer, 0, result.Count);

                    if (result.EndOfMessage)
                    {
                        string jsonResponse = Encoding.UTF8.GetString(memoryStream.ToArray());
                        Debug.Log($"接收到完整数据: {jsonResponse}");
                        memoryStream.SetLength(0); // 重置流
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
                Debug.Log("[DEBUG] 原始响应JSON: " + json);
                // 1. 检查 response 是否为空
                if (response == null)
                {
                    Debug.LogError("反序列化失败，响应为空");
                    return;
                }

                // 2. 检查 header 是否存在
                if (response.header == null)
                {
                    Debug.LogError("响应缺少 header");
                    return;
                }

                // 3. 处理错误码
                if (response.header.code != 0)
                {
                    Debug.LogError($"识别错误: {response.header.code} - {response.header.message}");
                    return;
                }

                // 4. 检查 payload 和 result
                if (response.payload?.result == null)
                {
                    Debug.Log("无有效结果数据");
                    return;
                }

                // 5. 检查 text 字段
                if (string.IsNullOrEmpty(response.payload.result.text))
                {
                    Debug.Log("结果文本为空");
                    return;
                }

                // 安全访问
                string decodedText = Encoding.UTF8.GetString(
                    Convert.FromBase64String(response.payload.result.text)
                );
                realtimeText.Append(decodedText);
                Debug.Log($"识别结果: {realtimeText}");
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
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "User requested close",
                            CancellationToken.None
                        );
                    }
                    timeoutCTS.Cancel();
                    webSocket.Dispose();
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
            isRecording = false; // 停止音频采集

            // 延迟等待最终结果
            StartCoroutine(WaitForFinalResult());
            Microphone.End(null); // 停止麦克风录音
        }

        IEnumerator WaitForFinalResult()
        {
            float timeout = 5.0f; // 最多等待5秒
            float startTime = Time.time;

            while (!isFinalResultReceived && Time.time - startTime < timeout)
            {
                yield return null;
            }

            CloseWebSocket();
        }
        #endregion
    }
}
