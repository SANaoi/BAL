using UnityEngine;
using GameEntry = Aki.Scripts.Base.GameEntry;
using Aki.Scripts.UI;
using Aki.Scripts.Definition.Constant;
using UnityGameFramework.Runtime;
using System.Threading.Tasks;
using Aki.Scripts.Camera;

namespace Aki.Scripts.Entities
{
    public class CharacterInteractableLogic : InteractableEntityLogic
    {
        private Animator m_Animator;
        private CharacterData characterData;
        public Transform CameraPoint;
        private CameraControl cameraControl;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_Animator = GetComponent<Animator>();
            CameraPoint = GameObject.Find("CameraPoint").transform;
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

        public override void OnInteract(object userData = null)
        {
            base.OnInteract();

            if (characterData != null)
            {
                GameEntry.UI.OpenUIForm(EnumUIForm.AIChatForm, this); 
                // GameEntry.DataNode.GetOrAddNode(Constant.ProcedureRunningData.AIChatFormSerialId).SetData<VarInt32>(AIChatFormId);
                IsInteractable = false;

                if (userData is PlayerLogic PlayerLogic)
                {
                    PlayerLogic.RemoveInputActionsCallbacks();
                    PlayerLogic.gameObject.SetActive(false);
                    cameraControl = GameEntry.Entity.GetEntity(PlayerLogic.cameraData.entityId).Logic as CameraControl;
                    cameraControl.OnInteract(CameraPoint);
                }
            }
            else
            {
                Debug.LogWarning("CharacterData is null");
            }
        }

    }
}
