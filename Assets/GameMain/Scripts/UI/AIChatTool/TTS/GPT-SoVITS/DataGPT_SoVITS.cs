using System.Collections.Generic;
using System;


namespace Aki.Scripts.UI
{
    [Serializable]
    public class DataGPT_SoVITS
    {
        // 必需参数
        public string text; // [必需] 要合成的文本内容（支持多语言混合）
        public string text_lang; // [必需] 输入文本的语种标识（如：zh、ja、en）
        public string ref_audio_path; // [必需] 参考音频文件路径（需绝对路径）
        public string prompt_lang; // [必需] 参考音频提示文本的语言标识

        // 可选参数
        public List<string> aux_ref_audio_paths = new List<string>(); // [可选] 多说话人辅助参考音频路径列表
        public string prompt_text; // [可选] 参考音频对应的文本提示

        // 算法参数（带默认值）
        public int top_k = 5; // [默认5] Top-K采样值，影响生成多样性
        public float top_p = 1.0f; // [默认1.0] Top-P采样概率阈值
        public float temperature = 1.0f; // [默认1.0] 温度参数控制随机性
        public string text_split_method = "cut2"; // [默认cut0] 文本分段处理方法
        public int batch_size = 1; // [默认1] 批处理大小
        public float batch_threshold = 0.75f; // [默认0.75] 批处理分割阈值
        public bool split_bucket = true; // [默认true] 是否将批处理分割为多个桶
        public bool return_fragment; // [默认false] 是否逐步返回音频片段
        public float speed_factor = 1.0f; // [默认1.0] 音频合成速度控制因子
        public bool streaming_mode; // [默认false] 是否启用流式响应模式
        public int seed = -1; // [默认-1] 随机种子（用于结果复现）
        public bool parallel_infer = true; // [默认true] 是否启用并行推理
        public float repetition_penalty = 1.35f; // [默认1.35] 重复惩罚系数
    }
}
