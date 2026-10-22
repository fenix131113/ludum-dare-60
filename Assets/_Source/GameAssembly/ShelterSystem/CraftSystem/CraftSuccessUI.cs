using System;
using System.Collections;
using DG.Tweening;
using GameAssembly.ItemsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftSuccessUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image itemIcon;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private float displayDuration = 2.0f;
        [SerializeField] private string textFormat = "You Crafted\n{0}";

        private void Awake()
        {
            EnsureUiComponents();
            HideItemIcon();
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = 0f;
                overlayCanvasGroup.gameObject.SetActive(false);
            }
        }

        public void PlaySuccessAnimation(ItemDataSO item, Action onFinished)
        {
            EnsureUiComponents();
            StartCoroutine(SuccessRoutine(item, onFinished));
        }

        private IEnumerator SuccessRoutine(ItemDataSO item, Action onFinished)
        {
            if (overlayCanvasGroup == null)
            {
                onFinished?.Invoke();
                yield break;
            }

            overlayCanvasGroup.gameObject.SetActive(true);
            overlayCanvasGroup.alpha = 0f;

            if (titleText != null)
            {
                titleText.text = string.Format(textFormat, item ? item.ItemName.ToUpper() : "ITEM");
                titleText.alpha = 0f;
                titleText.transform.localScale = Vector3.one * 0.8f;
            }

            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(true);
                itemIcon.sprite = item ? item.Icon : null;
                itemIcon.enabled = itemIcon.sprite != null;
                itemIcon.color = new Color(1f, 1f, 1f, 0f);
                itemIcon.transform.localScale = Vector3.one * 0.8f;
            }

            // 1. Fade screen to dark
            overlayCanvasGroup.DOFade(1f, fadeDuration);

            // 2. Animate text in
            if (titleText != null)
            {
                titleText.DOFade(1f, fadeDuration);
                titleText.transform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack);
            }

            if (itemIcon != null && itemIcon.enabled)
            {
                itemIcon.DOFade(1f, fadeDuration);
                itemIcon.transform.DOScale(Vector3.one, fadeDuration).SetEase(Ease.OutBack);
            }

            yield return new WaitForSeconds(fadeDuration + displayDuration);

            // 3. Fade screen back out
            var fadeOut = overlayCanvasGroup.DOFade(0f, fadeDuration);
            if (titleText != null)
            {
                titleText.DOFade(0f, fadeDuration);
            }

            if (itemIcon != null && itemIcon.enabled)
                itemIcon.DOFade(0f, fadeDuration);

            yield return fadeOut.WaitForCompletion();

            HideItemIcon();
            overlayCanvasGroup.gameObject.SetActive(false);
            onFinished?.Invoke();
        }

        private void HideItemIcon()
        {
            if (itemIcon == null)
                return;

            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemIcon.color = new Color(1f, 1f, 1f, 0f);
            itemIcon.gameObject.SetActive(false);
        }

        private void EnsureUiComponents()
        {
            if (overlayCanvasGroup != null && titleText != null && itemIcon != null)
                return;

            // Search in children first
            if (overlayCanvasGroup == null)
                overlayCanvasGroup = GetComponentInChildren<CanvasGroup>(true);

            if (titleText == null)
                titleText = GetComponentInChildren<TMP_Text>(true);

            // Auto-create dynamically if not set up in scene
            if (overlayCanvasGroup == null)
            {
                var canvas = FindAnyObjectByType<Canvas>();
                if (canvas == null)
                {
                    var canvasObj = new GameObject("CraftSuccessCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    canvas = canvasObj.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }

                var panel = new GameObject("CraftSuccessOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                panel.transform.SetParent(canvas.transform, false);

                var rect = panel.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var img = panel.GetComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0.95f);

                overlayCanvasGroup = panel.GetComponent<CanvasGroup>();

            }

            if (titleText == null)
            {
                var textObj = new GameObject("CraftSuccessText", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(overlayCanvasGroup.transform, false);
                titleText = textObj.GetComponent<TextMeshProUGUI>();
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.fontSize = 48;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = Color.white;
            }

            var textRect = titleText.rectTransform;
            textRect.anchorMin = new Vector2(0.1f, 0.54f);
            textRect.anchorMax = new Vector2(0.9f, 0.82f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            if (itemIcon == null && overlayCanvasGroup != null)
                itemIcon = overlayCanvasGroup.transform.Find("CraftSuccessIcon")?.GetComponent<Image>();

            if (itemIcon == null && overlayCanvasGroup != null)
            {
                var iconObj = new GameObject("CraftSuccessIcon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(overlayCanvasGroup.transform, false);
                itemIcon = iconObj.GetComponent<Image>();
                itemIcon.preserveAspect = true;
                itemIcon.raycastTarget = false;
            }

            if (itemIcon != null)
            {
                var iconRect = itemIcon.rectTransform;
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = new Vector2(0f, -40f);
                iconRect.sizeDelta = new Vector2(280f, 280f);
                itemIcon.preserveAspect = true;
                itemIcon.raycastTarget = false;
            }
        }
    }
}
