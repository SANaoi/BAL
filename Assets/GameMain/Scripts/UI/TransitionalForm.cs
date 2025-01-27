using System;
using Aki.Definition.Constant;
using Aki.GameArgs;
using DG.Tweening;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Aki.UI
{
    public class TransitionalForm : UGuiForm
    {
        /// <summary>
        /// 文字提示
        /// </summary>
        private Text m_Text;

        /// <summary>
        /// 用于调节透明度实现渐变效果
        /// </summary>
        private CanvasGroup m_CanvasGroup;

        /// <summary>
        /// 将要展示的资源已初始化完成
        /// </summary>
        private bool m_NextResourceSuccess;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_CanvasGroup = GetComponent<CanvasGroup>();
            m_Text = transform.Find("Text").GetComponent<Text>();
        }
        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            // GameEntry.Sound.PlaySound(1);
            m_CanvasGroup.alpha = 1;
            m_Text.text = GameEntry.DataNode.GetData<VarString>(Constant.ProcedureRunningData.TransitionalMessage);
            //订阅资源加载完毕的事件
            GameEntry.Event.Subscribe(LoadNextResourcesSuccessArgs.EventId, HideUI);
        }

        /// <summary>
        /// 资源加载成功后的回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideUI(object sender, GameEventArgs e)
        {
            LoadNextResourcesSuccessArgs ne = (LoadNextResourcesSuccessArgs) e;
            if (ne.LoadNextResourcesSuccess)
            {
                m_CanvasGroup.DOFade(0, 0.7f).OnComplete(() => GameEntry.UI.CloseUIForm(UIForm));
            }
        }
        protected override void OnClose(object userData)
        {
            GameEntry.Event.Unsubscribe(LoadNextResourcesSuccessArgs.EventId, HideUI);
            base.OnClose(userData);
        }
    }
}