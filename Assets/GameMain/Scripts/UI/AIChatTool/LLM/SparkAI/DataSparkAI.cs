using System.Collections.Generic;
using UnityEngine;

namespace Aki.Scripts.UI
{
    public class DataSparkAI
    {
        //构造请求体
        [System.Serializable]
        public class JsonRequest
        {
            public Header header { get; set; }
            public Parameter parameter { get; set; }
            public Payload payload { get; set; }
        }
        [System.Serializable]
        public class Header
        {
            public string app_id { get; set; }
            public string uid { get; set; }
        }
        [System.Serializable]
        public class Parameter
        {
            public Chat chat { get; set; }
        }
        [System.Serializable]
        public class Chat
        {
            public string domain { get; set; }
            public double temperature { get; set; }
            public int max_tokens { get; set; }
        }
        [System.Serializable]
        public class Payload
        {
            public Message message { get; set; }
        }
        [System.Serializable]
        public class Message
        {
            public List<Content> text { get; set; }
        }
        [System.Serializable]
        public class Content
        {
            public string role { get; set; }
            public string content { get; set; }
        }
        //接收的数据
        [System.Serializable]
        public class ResponseData
        {
            public ReHeaderData header = new ReHeaderData();
            public PayloadData payload = new PayloadData();
        }
        [System.Serializable]
        public class ReHeaderData
        {
            public int code;//错误码，0表示正常，非0表示出错
            public string message = string.Empty;//会话是否成功的描述信息
            public string sid = string.Empty;
            public int status;//会话状态，取值为[0,1,2]；0代表首次结果；1代表中间结果；2代表最后一个结果
        }
        [System.Serializable]
        public class PayloadData
        {
            public ChoicesData choices = new ChoicesData();
            //usage 开发者自行扩展
        }
        [System.Serializable]
        public class ChoicesData
        {
            public int status;
            public int seq;
            public List<ReTextData> text = new List<ReTextData>();
        }
        [System.Serializable]
        public class ReTextData
        {
            public string content = string.Empty;
            public string role = string.Empty;
            public int index;
        }
    }

}