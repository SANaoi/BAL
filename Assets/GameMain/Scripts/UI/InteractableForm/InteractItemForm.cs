using Aki.Scripts.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Aki.Scripts.UI
{
    public class InteractItemForm : UGuiForm
    {
        public Text ItemName;
        private EntityData entityData;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            entityData = userData as EntityData;
        }

        protected override void OnOpen(object userData)
        {
            ItemName.text = entityData.AssetName;
        }
    }
}
