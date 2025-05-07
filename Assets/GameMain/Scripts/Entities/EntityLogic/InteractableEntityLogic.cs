
using Aki.Scripts.UI;
using Aki.Scripts.Definition.Constant;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;
using GameFramework.Event;

namespace Aki.Scripts.Entities
{
    public class InteractableEntityLogic : DefaultEntityLogic
    {

        [Header("UI References")]
        public InteractableForm interactableForm; // 交互主界面
        public InteractItemForm interactItemForm; // 与实体绑定的交互单元界面
        private int m_interactItemFormSerialId;
        private int m_interactableFormSerialId;

        public bool IsActive { get; protected set; } = true;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            GameEntry.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

        }
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            m_interactableFormSerialId = GameEntry.DataNode.GetData<VarInt32>(Constant.ProcedureRunningData.InteractableUISerialId);
            m_interactItemFormSerialId = (int)GameEntry.UI.OpenUIForm(EnumUIForm.InteractItemForm);
        }
        protected override void OnRecycle()
        {
            GameEntry.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
        }

        void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            interactableForm = (InteractableForm)GameEntry.UI.GetUIForm(m_interactableFormSerialId).Logic;
            interactItemForm = (InteractItemForm)GameEntry.UI.GetUIForm(m_interactItemFormSerialId).Logic;
            interactableForm.Visible = true;
            interactItemForm.Visible = false;

            interactableForm.AddInteractableItem(interactItemForm);
        }

        public virtual void OnEnterRange()
        {
            if (interactItemForm != null)
            {
                interactItemForm.Visible = true;
            }
        }
        public virtual void OnExitRange()
        {
            if (interactItemForm != null)
            {
                interactItemForm.Visible = false;
            }
        }
        // 交互逻辑
        public virtual void OnInteract()
        {
        }

    }

}