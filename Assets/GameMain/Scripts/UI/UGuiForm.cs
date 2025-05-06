
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameEntry = Aki.Scripts.Base.GameEntry;

namespace Aki.Scripts.UI
{
    public class UGuiForm : UIFormLogic
    {
        private Canvas m_CachedCanvas = null;
        private CanvasGroup m_CanvasGroup = null;
        private const float FadeTime = 0.3f;
        public const int DepthFactor = 100;
        public int OriginalDepth
        {
            get;
            private set;
        }

        public int Depth
        {
            get
            {
                return m_CachedCanvas.sortingOrder;
            }
        }
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_CachedCanvas = gameObject.GetOrAddComponent<Canvas>();
            m_CachedCanvas.overrideSorting = true;
            OriginalDepth = m_CachedCanvas.sortingOrder;

            m_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

            RectTransform transform = GetComponent<RectTransform>();
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = Vector2.zero;

            gameObject.GetOrAddComponent<GraphicRaycaster>();
        }
        
        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            int oldDepth = Depth;
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            int deltaDepth = UGuiGroupHelper.DepthFactor * uiGroupDepth + DepthFactor * depthInUIGroup - oldDepth +
                             OriginalDepth;
            Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].sortingOrder += deltaDepth;
            }
        }
        public void Close(bool ignoreFade)
        {
            StopAllCoroutines();

            if (ignoreFade)
            {
                GameEntry.UI.CloseUIForm(this.UIForm);
            }
            else
            {
                StartCoroutine(CloseCo(FadeTime));
            }
        }
        
        private IEnumerator CloseCo(float duration)
        {
            yield return m_CanvasGroup.DOFade(0f, duration);
            GameEntry.UI.CloseUIForm(this.UIForm);
        }
    }
}