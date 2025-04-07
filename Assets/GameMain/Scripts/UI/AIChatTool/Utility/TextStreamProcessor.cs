using System.Text;

namespace Aki.Scripts.UI
{
    public class TextStreamProcessor
    {
        // 定义语句结束标识符（支持中英文）
        private readonly char[] punctuationMarks = { ',', '.', '!', '?', '，', '。', '！', '？' };

        // 文本缓冲区
        private StringBuilder textBuffer = new StringBuilder();

        public string ReceiveTextChunk(string chunk)
        {
            textBuffer.Append(chunk);
            return CheckAndOutputCompleteSentences();
        }

        private string CheckAndOutputCompleteSentences()
        {
            int cutPosition = -1;

            // 查找最后的停顿符号位置
            for (int i = 0; i < textBuffer.Length; i++)
            {
                if (IsPunctuation(textBuffer[i]))
                {
                    cutPosition = i + 1;
                }
            }

            // 发现完整语句则切割输出
            if (cutPosition != -1)
            {
                string completeSentence = textBuffer.ToString(0, cutPosition);
                textBuffer.Remove(0, cutPosition);
                return completeSentence;
            }

            return null;
        }

        private bool IsPunctuation(char c)
        {
            foreach (char mark in punctuationMarks)
            {
                if (c == mark) return true;
            }
            return false;
        }
        
        // 强制输出剩余内容（用于退出时）
        public string ForceFlush()
        {
            var result = textBuffer.ToString();
            textBuffer.Clear();
            return result;
        }
    }
}