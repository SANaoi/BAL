using System;
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
                public string result;
                public int status; // 0:中间结果, 1:最终结果
            }
        }
    }
}
