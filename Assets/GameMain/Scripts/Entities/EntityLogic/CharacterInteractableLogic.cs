using Aki.Scripts.Base;
using Aki.Scripts.UI;
using Aki.Scripts.Definition.Constant;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;
using System;

namespace Aki.Scripts.Entities
{
    public class CharacterInteractableLogic : InteractableEntityLogic
    {
        [SerializeField] private InteractableForm interactableForm;
        private int interactableFormSerialId;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            interactableFormSerialId = GameEntry.DataNode.GetData<VarInt32>(Constant.ProcedureRunningData.IntractableUISerialId);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            interactableForm = (InteractableForm)GameEntry.UI.GetUIForm(interactableFormSerialId).Logic;
        }
    }
}
