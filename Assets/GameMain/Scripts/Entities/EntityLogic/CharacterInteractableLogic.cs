using UnityEngine;
using GameEntry = Aki.Scripts.Base.GameEntry;
using Aki.Scripts.UI;
using Aki.Scripts.Definition.Constant;
using UnityGameFramework.Runtime;
using System.Threading.Tasks;

namespace Aki.Scripts.Entities
{
    public class CharacterInteractableLogic : InteractableEntityLogic
    {
        private Animator m_Animator;
        private CharacterData characterData;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_Animator = GetComponent<Animator>();

            characterData = userData as CharacterData;
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            m_Animator.SetBool("OnShow", true);
        }
        public override void OnEnterRange()
        {
            base.OnEnterRange();
        }

        public override void OnExitRange()
        {
            base.OnExitRange();
        }

        public override void OnInteract()
        {
            base.OnInteract();

            if (characterData != null)
            {
                GameEntry.UI.OpenUIForm(EnumUIForm.AIChatForm, this); 
                // GameEntry.DataNode.GetOrAddNode(Constant.ProcedureRunningData.AIChatFormSerialId).SetData<VarInt32>(AIChatFormId);
            }
            else
            {
                Debug.LogWarning("CharacterData is null");
            }
        }

    }
}
