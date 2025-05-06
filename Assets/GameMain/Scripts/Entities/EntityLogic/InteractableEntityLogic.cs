using System.Collections;
using UnityEngine;
using Aki.Scripts.UI;
using GameEntry = Aki.Scripts.Base.GameEntry;
using UnityGameFramework.Runtime;

namespace Aki.Scripts.Entities
{
    public class InteractableEntityLogic : DefaultEntityLogic
    {

        [Header("UI References")]
        [SerializeField] protected UGuiForm interactionPrompt; // 交互提示UI
        [SerializeField] protected CanvasGroup targetUI; // 显示交互后的目标界面

        [Header("Interaction Settings")]
        [SerializeField] private float fadeDuration = 0.3f;
        private Coroutine fadeCoroutine;

        public bool IsActive {get; protected set;} = true;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // interactionPrompt = GameEntry.UI.HasUIForm
        }

        public virtual void OnEnterRange()
        {
            TogglePrompt(true);
        }
        public virtual void OnExitRange()
        {
            TogglePrompt(false);
            CloseUI();
        }
        
        // 交互逻辑
        public virtual void OnInteract() { }
        private void TogglePrompt(bool show) => interactionPrompt.gameObject.SetActive(show);

        protected void ToggleUI(bool show)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeUI(show ? 1 : 0));
        }

        private IEnumerator FadeUI(float targetAlpha)
        {
            float elapsed = 0;
            float startAlpha = targetUI.alpha;

            while (elapsed < fadeDuration)
            {
                targetUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            targetUI.alpha = targetAlpha;
            targetUI.interactable = targetAlpha > 0.9f;
            targetUI.blocksRaycasts = targetAlpha > 0.9f;
        }
        private void CloseUI() => ToggleUI(false);
    }

}