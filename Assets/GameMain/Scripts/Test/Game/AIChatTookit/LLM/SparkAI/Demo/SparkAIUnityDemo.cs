using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

using static Aki.AI.LLM;


namespace Aki.AI.Demo
{
    public class SparkAIUnityDemo : MonoBehaviour
    {
        /// <summary>
        /// 计算方法调用的时间
        /// </summary>
        [SerializeField] protected Stopwatch stopwatch= new Stopwatch();
        /// <summary>
        /// 缓存对话
        /// </summary>
        [SerializeField] public List<SendData> m_DataList = new List<SendData>();
        /// <summary>
        /// 发送按钮
        /// </summary>
        public Button m_CommitMsgBtn;
        [SerializeField] private const string m_appid = "064b0378";
        [SerializeField] private const string api_secret = "MjNhM2FhYzY0OWQ5ZGM5MjJkZjdkNjFh";
        [SerializeField] private const string api_key = "98989f21fb7430bf845fc191784b70fd";
        private static string hostUrl = "https://spark-api.xf-yun.com/v1.1/chat";
        private ClientWebSocket m_webSocket;
        private CancellationToken m_cancellation;

        private void Start()
        {
            m_CommitMsgBtn.onClick.AddListener(SendOnClickButton);
        }

        public async void Tasker(Action<string> _callback)
        {
            try 
            {
                stopwatch.Restart();

                m_webSocket = new ClientWebSocket();
                m_cancellation = new CancellationToken();

                string authUrl = GetAuthUrl();
                string url = authUrl.Replace("http://", "ws://").Replace("https://", "wss://");
                Uri uri = new Uri(url);

                await m_webSocket.ConnectAsync(uri, m_cancellation);

                JsonRequest request = SendJsonContent("");

                string jsonString = JsonConvert.SerializeObject(request);;

                var frameData2 = Encoding.UTF8.GetBytes(jsonString.ToString());

                await m_webSocket.SendAsync(new ArraySegment<byte>(frameData2), WebSocketMessageType.Text, true, m_cancellation);
                
                StringBuilder sb = new StringBuilder();
                //用于拼接返回的答复
                string _callBackMessage = "";
                
                while (m_webSocket.State == WebSocketState.Open)
                {
                    var result = new byte[4096];
                    //接受数据
                    await m_webSocket.ReceiveAsync(new ArraySegment<byte>(result), m_cancellation);
                    //去除空字节  
                    List<byte> list = new List<byte>(result); 
                    while (list[list.Count - 1] == 0x00) list.RemoveAt(list.Count - 1);
                    
                    var str = Encoding.UTF8.GetString(list.ToArray());
                    sb.Append(str);
                    if (str.EndsWith("}"))
                    {
                        //获取返回的数据
                        UnityEngine.Debug.Log(sb);

                        ResponseData _responseData = JsonUtility.FromJson<ResponseData>(sb.ToString());
                        UnityEngine.Debug.Log(_responseData);
                        sb.Clear();

                        if (_responseData.header.code != 0)
                        {
                            //返回错误
                            UnityEngine.Debug.Log("错误码：" + _responseData.header.code);
                            m_webSocket.Abort();
                            break;
                        }
                        //没有回复数据
                        if (_responseData.payload.choices.text.Count == 0)
                        {
                            UnityEngine.Debug.LogError("没有获取到回复的信息！");
                            m_webSocket.Abort();
                            break;
                        }
                        //拼接回复的数据
                        _callBackMessage += _responseData.payload.choices.text[0].content;

                        if (_responseData.payload.choices.status == 2)
                        {
                            stopwatch.Stop();
                            UnityEngine.Debug.Log("ChatSpark耗时: " + stopwatch.Elapsed.TotalSeconds);

                            //添加记录
                            m_DataList.Add(new SendData("assistant", _callBackMessage));

                            //回调
                            _callback(_callBackMessage);
                            m_webSocket.Abort();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("报错信息: " + e.Message);
                m_webSocket.Dispose();
            }
        }
        
        // 返回code为错误码时，请查询https://www.xfyun.cn/document/error-code解决方案
        private string GetAuthUrl()
        {
            string date = DateTime.UtcNow.ToString("r");

            Uri uri = new Uri(hostUrl);
            StringBuilder builder = new StringBuilder("host: ").Append(uri.Host).Append("\n").//
                                    Append("date: ").Append(date).Append("\n").//
                                    Append("GET ").Append(uri.LocalPath).Append(" HTTP/1.1");

            string sha = HMACsha256(api_secret, builder.ToString());
            string authorization = string.Format("api_key=\"{0}\", algorithm=\"{1}\", headers=\"{2}\", signature=\"{3}\"", api_key, "hmac-sha256", "host date request-line", sha);
            //System.Web.HttpUtility.UrlEncode

            string NewUrl = "https://" + uri.Host + uri.LocalPath;

            string path1 = "authorization" + "=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authorization));
            date = date.Replace(" ", "%20").Replace(":", "%3A").Replace(",", "%2C");
            string path2 = "date" + "=" + date;
            string path3 = "host" + "=" + uri.Host;

            NewUrl = NewUrl + "?" + path1 + "&" + path2 + "&" + path3;
            return NewUrl;
        }

        public string HMACsha256(string apiSecretIsKey, string buider)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(apiSecretIsKey);
            System.Security.Cryptography.HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
            byte[] date = System.Text.Encoding.UTF8.GetBytes(buider);
            date = hMACSHA256.ComputeHash(date);
            hMACSHA256.Clear();

            return Convert.ToBase64String(date);
        }
        
        private JsonRequest SendJsonContent(string text)
        {
            JsonRequest request = new JsonRequest();
            request.header = new Header()
                                {
                                    app_id = m_appid,
                                    uid = "admin" //选 填，用户的ID
                                };
            request.parameter = new Parameter()
                                {
                                    chat = new Chat()
                                    {
                                        domain = "lite",//模型领域，默认为星火通用大模型
                                        temperature = 0.5,//温度采样阈值，用于控制生成内容的随机性和多样性，值越大多样性越高；范围（0，1）
                                        max_tokens = 1024,//生成内容的最大长度，范围（0，4096）
                                    }
                                };
            request.payload = new Payload()
                                {
                                    message = new Message()
                                    {
                                        text = new List<Content>
                                        {
                                            new Content() { role = "user", content = "你是谁" },
                                            // new Content() { role = "assistant", content = "....." }, // AI的历史回答结果，这里省略了具体内容，可以根据需要添加更多历史对话信息和最新问题的内容。
                                        }
                                    }
                                };
            return request;
        }
        
        public void SendOnClickButton()
        {
            Tasker(CallBack);
        }
        /// <summary>
        /// AI回复的信息的回调
        /// </summary>
        /// <param name="_response"></param>
        private void CallBack(string _response)
        {
            UnityEngine.Debug.Log(_response);
        }
    }
}