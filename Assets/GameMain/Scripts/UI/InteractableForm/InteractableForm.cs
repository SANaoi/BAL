using System.Collections;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityEngine.UI;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.UI
{
    public class InteractableForm : UGuiForm
    {
        public GameObject ScrollViewContent; 
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
        }

        protected override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
        }

        public void AddInteractableItem(InteractItemForm interactItemForm)
        {
            if (ScrollViewContent == null)
            {
                Log.Error("ScrollViewContent is null.");
                return;
            }

            interactItemForm.transform.SetParent(ScrollViewContent.transform, false); 
        }
    }
}