using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Aki.Scripts.UI
{
    public class GPT_SoVITS : TTS
    {
        #region 参数定义
        
        [Header("参考音频的文字内容，必须设置")]
        [SerializeField] private string m_ReferenceText = "";//参考音频文本
        [Header("参考音频的语言")]
        [SerializeField] private string m_ReferenceTextLan = "ja";//参考音频的语言
        [Header("合成音频的语言")]
        [SerializeField] private string m_TargetTextLan = "zh";//合成音频的语言
        private string m_Audio2Path = "";//参考音频的路径
        #endregion

        private void Start()
        {
            m_PostURL = "http://127.0.0.1:9880/tts";
        }

        public override void Speak(string _msg, Action<AudioClip, string> _callback)
        {
            base.Speak(_msg, _callback);

            StartCoroutine(GetVoice(_msg , _callback));
        }

        /// <summary>
        /// 合成音频
        /// </summary>
        /// <param name="_msg"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        private IEnumerator GetVoice(string _msg, Action<AudioClip, string> _callback)
        {
            stopwatch.Restart();

            string json = GetPostJson(_msg);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to generate valid JSON.");
                yield break;
            }
            Debug.Log("Request JSON: " + json);
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(json);
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(m_PostURL, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(postData);
                request.downloadHandler = new DownloadHandlerAudioClip(m_PostURL, AudioType.WAV);
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    _callback?.Invoke(clip, _msg);
                }
                else
                {
                    Debug.LogError($"Error: {request.error}\nResponse Code: {request.responseCode}");
                }
            }

            stopwatch.Stop();
            Debug.Log($"GPT-SoVITS合成耗时：{stopwatch.Elapsed.TotalSeconds:F2}秒");
        }

        /// <summary>
        /// 处理发送的Json报文
        /// </summary>
        /// <param name="_msg"></param>
        /// <param name="_lan"></param>
        /// <returns></returns>
        private string GetPostJson(string _msg)
        {
            if (string.IsNullOrEmpty(m_ReferenceText) || string.IsNullOrEmpty(m_Audio2Path))
            {
                Debug.LogError("Missing reference audio/text configuration!");
                return null;
            }

            // 确保语言参数转换为小写（根据API要求）
            string textLang = m_TargetTextLan.ToString().ToLower();
            string promptLang = m_ReferenceTextLan.ToString().ToLower();

            DataGPT_SoVITS data = new DataGPT_SoVITS
            {
                text = _msg,
                text_lang = textLang,
                ref_audio_path = m_Audio2Path,
                prompt_text = m_ReferenceText,
                prompt_lang = promptLang,
                text_split_method = "cut2"
            };

            return JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// 从本地获取合成后的音频文件
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="_msg"></param>
        /// <param name="_callback"></param>
        /// <returns></returns>
        private void GetAudioFromFile(byte[] audioData, string _msg, Action<AudioClip, string> _callback)
        {
            AudioClip audioClip = ToAudioClip(audioData);
            _callback(audioClip, _msg);

            return;
        }

        #region 数据定义
        /*
         发送的数据格式

        """
        # WebAPI文档

        ` python api_v2.py -a 127.0.0.1 -p 9880 -c GPT_SoVITS/configs/tts_infer.yaml `

        ## 执行参数:
            `-a` - `绑定地址, 默认"127.0.0.1"`
            `-p` - `绑定端口, 默认9880`
            `-c` - `TTS配置文件路径, 默认"GPT_SoVITS/configs/tts_infer.yaml"`

        ## 调用:

        ### 推理

        endpoint: `/tts`
        GET:
        ```
        http://127.0.0.1:9880/tts?text=先帝创业未半而中道崩殂，今天下三分，益州疲弊，此诚危急存亡之秋也。&text_lang=zh&ref_audio_path=processed_0021.wav&prompt_lang=ja&prompt_text=先生、どこかお掃除するところはありますか?&text_split_method=cut5&batch_size=1&media_type=wav&streaming_mode=true
        ```

        POST:
        ```json
        {
            "text": "",                   # str.(required) text to be synthesized
            "text_lang: "",               # str.(required) language of the text to be synthesized
            "ref_audio_path": "",         # str.(required) reference audio path
            "aux_ref_audio_paths": [],    # list.(optional) auxiliary reference audio paths for multi-speaker synthesis
            "prompt_text": "",            # str.(optional) prompt text for the reference audio
            "prompt_lang": "",            # str.(required) language of the prompt text for the reference audio
            "top_k": 5,                   # int. top k sampling
            "top_p": 1,                   # float. top p sampling
            "temperature": 1,             # float. temperature for sampling
            "text_split_method": "cut0",  # str. text split method, see text_segmentation_method.py for details.
            "batch_size": 1,              # int. batch size for inference
            "batch_threshold": 0.75,      # float. threshold for batch splitting.
            "split_bucket: True,          # bool. whether to split the batch into multiple buckets.
            "return_fragment": False,     # bool. step by step return the audio fragment.
            "speed_factor":1.0,           # float. control the speed of the synthesized audio.
            "streaming_mode": False,      # bool. whether to return a streaming response.
            "seed": -1,                   # int. random seed for reproducibility.
            "parallel_infer": True,       # bool. whether to use parallel inference.
            "repetition_penalty": 1.35    # float. repetition penalty for T2S model.
        }

        */
        public static AudioClip ToAudioClip(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(memoryStream))
                {
                    // 解析 WAV 文件头
                    reader.ReadBytes(16); // 跳过 RIFF 头
                    int channels = reader.ReadInt16(); // 声道数
                    int sampleRate = reader.ReadInt32(); // 采样率
                    reader.ReadBytes(6); // 跳过其他信息
                    int bitDepth = reader.ReadInt16(); // 位深度
                    reader.ReadBytes(4); // 跳过 "data" 标识
                    int dataSize = reader.ReadInt32(); // 数据块大小

                    // 读取音频数据
                    float[] samples = new float[dataSize / 2];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        samples[i] = reader.ReadInt16() / 32768.0f; // 转换为 float
                    }

                    // 创建 AudioClip
                    AudioClip clip = AudioClip.Create("TTS Audio", samples.Length, channels, sampleRate, false);
                    clip.SetData(samples, 0);
                    return clip;
                }
            }
        }
        public void UploadAudio()
        {
            // 1. 用户选择文件
            NativeFilePicker.PickFile((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    m_Audio2Path = path;
                    StartCoroutine(SaveAndLoadAudio(path));
                }
            }, new string[] { "audio/wav", "audio/mpeg" });

        }

        IEnumerator SaveAndLoadAudio(string sourcePath)
        {
            // 2. 保存到本地
            byte[] bytes = File.ReadAllBytes(sourcePath);
            string fileName = "audio_" + System.Guid.NewGuid().ToString() + ".wav";
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(savePath, bytes);

            // 3. 加载并播放
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + savePath, AudioType.WAV);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            }
            else
            {
                Debug.LogError("加载失败: " + request.error);
            }
        }
        # endregion
    }

    public enum m_LanguageType
    {
        中文,
        英文,
        日文,
        中英混合,
        日英混合,
        多语种混合
    }

    public enum m_SplitType
    {
        不切,
        凑四句一切,
        凑50字一切,
        按中文句号一切,
        按英文句号一切,
        按标点符号切
    }
}