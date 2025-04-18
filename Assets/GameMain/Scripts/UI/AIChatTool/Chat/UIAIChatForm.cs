using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

using Aki.Scripts.Definition.Constant;

namespace Aki.Scripts.UI
{
    public class UIAIChatForm : MonoBehaviour
    {
        private void Start()
        {
            m_CommitMsgBtn.onClick.AddListener(SendData);
            m_VoiceInputBotton.onClick.AddListener(startStreamRecord);
        }

    #region UI定义
        /// <summary>
        /// 聊天配置
        /// </summary>
        [SerializeField] private ChatSetting m_ChatSettings;
        /// <summary>
        /// 聊天UI窗
        /// </summary>
        [SerializeField] private GameObject m_ChatPanel;
        /// <summary>
        /// 输入的信息
        /// </summary>
        [SerializeField] public InputField m_InputWord;
        /// <summary>
        /// 返回的信息
        /// </summary>
        [SerializeField] private Text m_TextBack;
        /// <summary>
        /// 播放声音
        /// </summary>
        [SerializeField] private AudioSource m_AudioSource;
        /// <summary>
        /// 发送信息按钮
        /// </summary>
        [SerializeField] private Button m_CommitMsgBtn;
    #endregion
    
    #region 参数定义
        /// <summary>
        /// 动画控制器
        /// </summary>
        [SerializeField] private Animator m_Animator;
        /// <summary>
        /// 语音模式，设置为false,则不通过语音合成
        /// </summary>
        [Header("设置是否通过语音合成播放文本")]
        [SerializeField] private bool m_IsVoiceMode = true;

        /// <summary>
        /// 语音合成的音频缓存列表
        /// </summary>
        private Queue<AudioClip> playbackQueue = new Queue<AudioClip>();

        /// <summary>
        /// 语音合成的文本流处理类
        /// </summary>
        protected TextStreamProcessor textStreamProcessor = new TextStreamProcessor();

    #endregion

    #region 消息发送

        /// <summary>
        /// 发送信息
        /// </summary>
        private void SendData()
        {
            if (m_InputWord.text.Equals(""))
            {
                return;
            }
            //判断是否在聊天中
            Constant.ChatData.IsChatting = true;
            
            //添加记录聊天
            m_ChatHistory.Add(m_InputWord.text);
            //提示词
            string _msg = m_InputWord.text;

            //发送数据
            m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

            m_InputWord.text = "";
            m_TextBack.text = "正在思考中...";

            //切换思考动作
        }

        /// <summary>
        /// </summary>
        /// <param name="_msg">语音生成文本</param>
        private void SendData(string _msg)
        {
            if (_msg.Equals("") || m_InputWord.text.Equals(""))
            {
                return;
            }
            //判断是否在聊天中
            Constant.ChatData.IsChatting = true;
            //添加记录聊天
            m_ChatHistory.Add(_msg);

            //发送数据
            m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

            m_InputWord.text = "";
            m_TextBack.text = "正在思考中...";

        }

        /// <summary>
        /// AI回复的信息的回调  
        /// </summary>
        /// <param name="_response"></param>
        protected virtual void CallBack(string _response)
        {
            _response = _response.Trim();
            string textChunk = textStreamProcessor.ReceiveTextChunk(_response);
            if (textChunk != null)
            {
                _response = textChunk;
            }
            else
            {
                return;
            }
            m_TextBack.text = "";

            Debug.Log("收到AI回复: "+ _response);
            
            //记录聊天
            m_ChatHistory.Add(_response);

            if (!m_IsVoiceMode||m_ChatSettings.m_TextToSpeech == null)
            {
                //开始逐个显示返回的文本
                StartTypeWords(_response);
                return;
            }

            m_ChatSettings.m_TextToSpeech.Speak(_response, PlayVoice);
        }
    #endregion

    #region 语音输入 

        /// <summary>
        /// 语音输入的按钮
        /// </summary>
        [SerializeField] private Button m_VoiceInputBotton;

        /// <summary>
        /// 录音的提示信息
        /// </summary>
        [SerializeField] private Text m_RecordTips;

        /// <summary>
        /// 是否发送文本信息
        /// </summary>
        [SerializeField] private bool m_AutoSend = true;

        bool m_isRecording = false;

        /// <summary>
        /// 流式录音
        /// </summary>
        /// <param name="_data"></param>
        private void startStreamRecord()
        {
            if (m_ChatSettings.m_SpeechToText == null)
                return;
            if (!m_isRecording)
            {
                m_ChatSettings.m_SpeechToText.SpeechToText(DealingTextCallback);
                m_RecordTips.text = "正在聆听...";
                m_isRecording = true;
            }
            else
            {
                m_ChatSettings.m_SpeechToText.SpeechToText(DealingTextCallback);
                m_RecordTips.text = "按住说话...";
                m_isRecording = false;
            }
        }
        /// <summary>
        /// 处理识别到的文本
        /// </summary>
        /// <param name="_msg"></param>
        private void DealingTextCallback(string _msg)
        {
            m_InputWord.text = _msg;
            // 自动发送
            if (m_AutoSend && !m_isRecording)
            {
                SendData(_msg);
                return;
            }

        }

    #endregion

    #region 语音合成

    private void PlayVoice(AudioClip _clip, string _response)
    {
        StartTypeWords(_response);
        EnqueueClip(_clip);
        Debug.Log("音频时长："+_response +"---"+ _clip.length);
        //开始逐个显示返回的文本
    }

    private void EnqueueClip(AudioClip _clip)
    {
        playbackQueue.Enqueue(_clip);
        if (!Constant.ChatData.IsSpeaking)
        {
            StartCoroutine(PlayQueueCoroutine());
        }
    }

    private IEnumerator PlayQueueCoroutine()
    {
        Constant.ChatData.IsSpeaking = true;
        while (playbackQueue.Count > 0)
        {
            AudioClip clip = playbackQueue.Dequeue();
            m_AudioSource.clip = clip;
            m_AudioSource.Play();

            yield return new WaitForSeconds(clip.length);
        }
        Constant.ChatData.IsSpeaking = false;
    }
    #endregion

    #region 聊天记录
        //保存聊天记录
        [SerializeField] private List<string> m_ChatHistory;
        //缓存已创建的聊天气泡
        [SerializeField] private List<GameObject> m_TempChatBox;
        //聊天记录显示层
        [SerializeField] private GameObject m_HistoryPanel;
        //聊天文本放置的层
        [SerializeField] private RectTransform m_rootTrans;
        //发送聊天气泡
        [SerializeField] private ChatPrefab m_PostChatPrefab;
        //回复的聊天气泡
        [SerializeField] private ChatPrefab m_RobotChatPrefab;
        //滚动条
        [SerializeField] private ScrollRect m_ScroTectObject;
        //获取聊天记录
        public void OpenAndGetHistory()
        {
            m_ChatPanel.SetActive(false);
            m_HistoryPanel.SetActive(true);

            ClearChatBox();
            StartCoroutine(GetHistoryChatInfo());
        }
        //返回
        public void BackChatMode()
        {
            m_ChatPanel.SetActive(true);
            m_HistoryPanel.SetActive(false);
        }

        //清空已创建的对话框
        private void ClearChatBox()
        {
            while (m_TempChatBox.Count != 0)
            {
                if (m_TempChatBox[0])
                {
                    Destroy(m_TempChatBox[0].gameObject);
                    m_TempChatBox.RemoveAt(0);
                }
            }
            m_TempChatBox.Clear();
        }

        //获取聊天记录列表
        private IEnumerator GetHistoryChatInfo()
        {

            yield return new WaitForEndOfFrame();

            for (int i = 0; i < m_ChatHistory.Count; i++)
            {
                if (i % 2 == 0)
                {
                    ChatPrefab _sendChat = Instantiate(m_PostChatPrefab, m_rootTrans.transform);
                    _sendChat.SetText(m_ChatHistory[i]);
                    m_TempChatBox.Add(_sendChat.gameObject);
                    continue;
                }

                ChatPrefab _reChat = Instantiate(m_RobotChatPrefab, m_rootTrans.transform);
                _reChat.SetText(m_ChatHistory[i]);
                m_TempChatBox.Add(_reChat.gameObject);
            }

            //重新计算容器尺寸
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
            StartCoroutine(TurnToLastLine());
        }

        private IEnumerator TurnToLastLine()
        {
            yield return new WaitForEndOfFrame();
            //滚动到最近的消息
            m_ScroTectObject.verticalNormalizedPosition = 0;
        }


    #endregion
    
    #region 文字逐个显示
        //逐字显示的时间间隔
        [SerializeField] private float m_WordWaitTime = 0.2f;
        //是否显示完成
        [SerializeField] private bool m_WriteState = false;
    
        /// <summary>
        /// 开始逐个打印
        /// </summary>
        /// <param name="_msg"></param>
        private void StartTypeWords(string _msg)
        {
            if (_msg == "")
                return;

            m_WriteState = true;
            StartCoroutine(SetTextPerWord(_msg));
        }

        private IEnumerator SetTextPerWord(string _msg)
        {
            int currentPos = 0;
            while (m_WriteState)
            {
                yield return new WaitForSeconds(m_WordWaitTime);
                currentPos++;
                //更新显示的内容
                m_TextBack.text = _msg.Substring(0, currentPos);

                m_WriteState = currentPos < _msg.Length;

            }

            //切换到等待动作
        }
    #endregion
    }
}