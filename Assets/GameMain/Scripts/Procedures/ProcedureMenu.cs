
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using Aki.Scripts.Definition.Constant;
using Aki.Scripts.UI;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;
using System;

namespace Aki.Procedures
{
    public class ProcedureMenu : ProcedureBase
    {
        /// <summary>
        /// 进入游戏流程的标识
        /// </summary>
        public bool ToGame { get; set; }

        /// <summary>
        /// 拿到主菜单UI引用
        /// </summary>
        private int m_MenuFormSerialId;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.DataNode.SetData<VarBoolean>(Constant.ProcedureRunningData.CanChangeProcedure, false);
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            m_MenuFormSerialId = (int)GameEntry.UI.OpenUIForm((int)EnumUIForm.MenuForm, this);

        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            // CanChangeProcedure 为 true 时，进入下一个流程
            if (GameEntry.DataNode.GetData<VarBoolean>(Constant.ProcedureRunningData.CanChangeProcedure))
            {
                ChangeState<ProcedureChangeScene>(procedureOwner);
            }
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            GameEntry.UI.CloseUIForm(m_MenuFormSerialId);
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

        }
    }
}