using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using DG.Tweening;
using UnityGameFramework.Runtime;
using Aki.Definition.Constant;

namespace Aki.UI
{
    public class MenuForm : UGuiForm
    {

        /// <summary>
        /// 开始游戏按钮
        /// </summary>
        private Transform m_StartGame;
        /// <summary>
        /// 储存选项的数组
        /// </summary>
        // private List<Transform> m_ListToSelect = new List<Transform>();

        /// <summary>
        /// 已经点击过space
        /// </summary>
        private bool m_HasHit;
        
        /// <summary>
        /// 用于调节透明度实现渐变效果
        /// </summary>
        private CanvasGroup m_CanvasGroup;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_StartGame = transform.Find("StartGame");
            m_StartGame.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClickStartGame);

            m_CanvasGroup = transform.GetComponent<CanvasGroup>();
            m_CanvasGroup.alpha = 0;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_CanvasGroup.DOFade(1.0f, 1.0f);
            m_HasHit = false;
            GameFrameworkLog.Info("Enter Menu");
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            //如果空格被点击过一次,就把所有操作屏蔽
            if (m_HasHit) return;
        }

        private void OnClickStartGame()
        {
            m_HasHit = true;
            GameEntry.DataNode.GetOrAddNode("PlayerMode").SetData<VarInt>(1);
            //TODO ProfileReader.Init(); 
            m_CanvasGroup.DOFade(0f, 1.0f).
                OnComplete(ToGame);
        }

        protected override void OnResume()
        {
            base.OnResume();
            m_HasHit = false;
            m_CanvasGroup.DOFade(1, 0.5f);
        }

        private void ToGame()
        {
            
            //打开过渡界面
            GameEntry.UI.OpenUIForm(UIFormId.TransitionalForm);
            //设置下一场景名称
            GameEntry.DataNode.SetData<VarString>(Constant.ProcedureRunningData.NextSceneName, "Game");
            //可以切换流程了
            GameEntry.DataNode.SetData<VarBool>(Constant.ProcedureRunningData.CanChangeProcedure, true);
            
        }
    }
}