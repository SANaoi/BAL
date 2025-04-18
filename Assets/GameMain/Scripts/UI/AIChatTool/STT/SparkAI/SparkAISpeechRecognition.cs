using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using WebSocketSharp;
using static Aki.Scripts.UI.DataSparkAISTT;

namespace Aki.Scripts.UI
{
    public class SparkAISpeechRecognition : STT
    {
        [Header("API Settings")]
        public string appId = "064b0378";
        public string apiKey = "98989f21fb7430bf845fc191784b70fd";
        public string apiSecret = "MjNhM2FhYzY0OWQ5ZGM5MjJkZjdkNjFh";

        private const string hostUrl = "wss://iat-api.xfyun.cn/v2/iat";
        private WebSocket ws;
        private AudioClip recordingClip;
        private int sampleRate = 16000;
        private bool isRecording = false;
        private readonly int frameSize = 1280; // 40ms的音频数据
        private readonly int statusFirstFrame = 0;
        private readonly int statusContinueFrame = 1;
        private readonly int statusLastFrame = 2;

        private bool isFirstFrameSent = false;
        public StringBuilder fullResult = new StringBuilder();

        private Action<string> m_callback = null;
        public override void SpeechToText(Action<string> _callback)
        {
            ToggleRecording();
            m_callback = _callback;
        }

        void ToggleRecording()
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        #region 鉴权方法
        string GetAuthUrl()
        {
            var date = DateTime.UtcNow.ToString("r");
            var signatureOrigin = $"host: iat-api.xfyun.cn\ndate: {date}\nGET /v2/iat HTTP/1.1";
            var signature = HmacSha256(apiSecret, signatureOrigin);
            var authorization = $"api_key=\"{apiKey}\", algorithm=\"hmac-sha256\", headers=\"host date request-line\", signature=\"{signature}\"";
            var authBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorization));
            return $"{hostUrl}?authorization={authBase64}&date={Uri.EscapeDataString(date)}&host=iat-api.xfyun.cn";
        }

        string HmacSha256(string secret, string data)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }
        #endregion

        #region 录音方法和通信方法
        void StartRecording()
        {
            fullResult.Clear();
            isRecording = true;

            // 初始化WebSocket
            ws = new WebSocket(GetAuthUrl());
            ws.OnMessage += OnWebSocketMessage;
            ws.Connect();
            Debug.Log("开始连接");

            // 开始录音
            recordingClip = Microphone.Start(null, true, 20, sampleRate);
            StartCoroutine(SendAudioData());
        }

        IEnumerator SendAudioData()
        {
            yield return new WaitForSeconds(0.1f); // 等待录音初始化

            int position = 0;
            isRecording = true;
            isFirstFrameSent = false;

            while (isRecording)
            {
                int currentPos = Microphone.GetPosition(null);
                if (currentPos < position) position = 0;

                if (currentPos - position >= frameSize)
                {
                    float[] samples = new float[frameSize];
                    recordingClip.GetData(samples, position);

                    // 转换为16位PCM
                    byte[] pcmBytes = ConvertAudioToPCM(samples);

                    // 构建请求
                    var request = new IatRequest();

                    if (!isFirstFrameSent)
                    {
                        request.common = new IatRequest.Common() { app_id = appId };
                        request.business = new IatRequest.Business();
                        request.data = new IatRequest.Data()
                        {
                            status = statusFirstFrame,
                            audio = Convert.ToBase64String(pcmBytes)
                        };
                        isFirstFrameSent = true;
                    }
                    else
                    {
                        request.data = new IatRequest.Data()
                        {
                            status = statusContinueFrame,
                            audio = Convert.ToBase64String(pcmBytes)
                        };
                    }

                    string json = JsonUtility.ToJson(request);
                    Debug.Log("当前消息发送状态： " + request.data.status);
                    ws.Send(json);

                    position += frameSize;
                }
                yield return new WaitForSeconds(0.04f);
            }

        }

        byte[] ConvertAudioToPCM(float[] samples)
        {
            byte[] pcm = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short value = (short)(samples[i] * short.MaxValue);
                BitConverter.GetBytes(value).CopyTo(pcm, i * 2);
            }
            return pcm;
        }
        #endregion

        #region WebSocket消息处理
        void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            var response = JsonUtility.FromJson<SparkResponse>(e.Data);
            Debug.Log($"Received message: {e.Data}");
            if (response.code != 0)
            {
                Debug.LogError($"Error {response.code}: {response.message}");
                return;
            }

            if (response.data.status == 2)
            {
                Debug.Log("response.data.status == 2");
                // StopRecording();
            }
            else
            {
                // 处理中间结果
                StringBuilder currentText = new StringBuilder();
                foreach (var ws in response.data.result.ws)
                {
                    foreach (var cw in ws.cw)
                    {
                        currentText.Append(cw.w);
                    }
                }
                fullResult.Append(currentText);

                m_callback?.Invoke(fullResult.ToString());
            }
        }
        #endregion

        #region 停止录音
        void StopRecording()
        {
            if (!isRecording) return;

            // 停止录音和协程
            isRecording = false;
            Microphone.End(null);
            StopCoroutine(SendAudioData());

            // 发送最后一帧（status=2）
            var endRequest = new IatRequest()
            {
                data = new IatRequest.Data()
                {
                    status = statusLastFrame,
                    audio = "",
                    format = "audio/L16;rate=16000",
                    encoding = "raw"
                }
            };
            ws.Send(JsonUtility.ToJson(endRequest));

            // 延迟关闭连接，确保最后一帧发送完成
            StartCoroutine(DelayCloseWebSocket());

            m_callback?.Invoke(fullResult.ToString());
        }

        IEnumerator DelayCloseWebSocket()
        {
            yield return new WaitForSeconds(0.5f);
            if (ws != null && ws.ReadyState == WebSocketState.Open)
            {
                ws.Close();
                ws = null;
            }
            Debug.Log("Sent final frame");
        }
        #endregion
    }
}
