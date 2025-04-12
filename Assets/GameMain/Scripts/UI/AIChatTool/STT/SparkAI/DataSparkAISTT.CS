using UnityEngine;

namespace Aki.Scripts.UI
{
    public class DataSparkAISTT
    {
        [System.Serializable]
        public class IatRequest
        {
            public Header header;
            public Parameter parameter;
            public Payload payload;

            [System.Serializable]
            public class Header
            {
                public string app_id;
                public int status; // 0:首帧 1:中间 2:尾帧
            }

            [System.Serializable]
            public class Parameter
            {
                public IatParams iat;

                [System.Serializable]
                public class IatParams
                {
                    public string domain = "iat";
                    public string language = "zh_cn";
                    public string accent = "mandarin";
                    public int eos = 6000;   // 静音检测超时（ms）
                    public int vinfo = 1;     // 返回中间结果
                    public string dwa = "wpgs";// 动态修正
                    public ResultParams result;

                    [System.Serializable]
                    public class ResultParams
                    {
                        public string encoding = "utf8";
                        public string compress = "raw";
                        public string format = "json";
                    }
                }
            }

            [System.Serializable]
            public class Payload
            {
                public AudioData audio;

                [System.Serializable]
                public class AudioData
                {
                    public string encoding = "raw";
                    public int sample_rate = 16000;
                    public int channels = 1;
                    public int bit_depth = 16;
                    public int seq;      // 帧序列号（从1递增）
                    public int status;   // 同header.status
                    public string audio; // Base64编码的PCM数据
                }
            }
        }
        [System.Serializable]
        public class SparkResponse
        {
            public Header header;
            public Payload payload;

            [System.Serializable]
            public class Header
            {
                public int code;      // 0表示成功
                public string message;
                public string sid;    // 会话ID
                public int status;    // 0:中间结果 1:尾帧
            }

            [System.Serializable]
            public class Payload
            {
                public Result result;

                [System.Serializable]
                public class Result
                {
                    public string compress;
                    public string encoding;
                    public string format;
                    public int seq;
                    public int status;
                    public string text; // Base64编码的实际文本
                }
            }
        }
    }
}
