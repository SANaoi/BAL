using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aki.Scripts.UI
{
    public class DataSparkAISTT
    {
        [Serializable]
        public class IatRequest
        {
            public Common common;
            public Business business;
            public Data data;

            [Serializable]
            public class Common
            {
                public string app_id;
            }

            [Serializable]
            public class Business
            {
                public string language = "zh_cn";
                public string domain = "iat";
                public string accent = "mandarin";
                public int vad_eos = 5000; // 静默时长设置
                public string dwa = "wpgs"; // 开启流式结果返回
            }

            [Serializable]
            public class Data
            {
                public int status;
                public string format = "audio/L16;rate=16000";
                public string audio;  // Base64编码的音频数据
                public string encoding = "raw";
            }
        }
        [System.Serializable]
        public class SparkResponse
        {
            public int code;
            public string message;
            public string sid;
            public ResponseData data;

            [Serializable]
            public class ResponseData
            {
                public result result;
                public int status; // 0:中间结果, 1:最终结果
            }

            [Serializable]
            public class result
            {
                [SerializeField] public List<WsTag> ws;
            }

            [Serializable]
            public class WsTag
            {
                [SerializeField] public List<CwTag> cw;
            }

            [Serializable]
            public class CwTag
            {
                [SerializeField] public int sc = 0;
                [SerializeField] public string w = string.Empty;
            }
        }
    }
}
