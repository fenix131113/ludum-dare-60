using GameAssembly.ItemsSystem;
using GameAssembly.Player;
using TMPro;
using UnityEngine;
using VContainer;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftItemTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private Vector2 padding = new(18f, 16f);
        [SerializeField] private Vector2 cursorOffset = new(20f, -20f);
        [SerializeField] private float minWidth = 220f;
        [SerializeField] private float maxWidth = 360f;
        [SerializeField] private float textSpacing = 10f;
        [SerializeField] private float screenMargin = 12f;

        [Inject] private PlayerState _playerState;

        private CraftCell _owner;
        private Vector2 _screenPosition;
        private Vector2 _canvasSize;
        private Camera _eventCamera;

        private void Awake()
        {
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
            Hide();
        }

        public void Show(CraftCell owner, ItemDataSO item, Vector2 screenPosition, Camera eventCamera)
        {
            Hide();

            if (!owner || !item || string.IsNullOrWhiteSpace(item.Description) || _playerState.IsMenuOpen())
                return;

            _owner = owner;
            titleLabel.text = item.ItemName;
            descriptionLabel.text = item.Description.Trim();
            Resize();
            Move(owner, screenPosition, eventCamera);
            panel.SetAsLastSibling();
            panelGroup.alpha = 1f;
        }

        public void Move(CraftCell owner, Vector2 screenPosition, Camera eventCamera)
        {
            if (_owner != owner)
                return;

            _screenPosition = screenPosition;
            _eventCamera = eventCamera;
            UpdatePosition();
        }

        public void Hide(CraftCell owner)
        {
            if (_owner == owner)
                Hide();
        }

        public void Hide()
        {
            _owner = null;
            panelGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            if (!_owner || !_owner.isActiveAndEnabled || _playerState.IsMenuOpen())
            {
                Hide();
                return;
            }

            if (_canvasSize != canvasRect.rect.size)
            {
                Resize();
                UpdatePosition();
            }
        }

        private void Resize()
        {
            _canvasSize = canvasRect.rect.size;
            var availableWidth = Mathf.Max(1f, _canvasSize.x - screenMargin * 2f);
            var preferredWidth = Mathf.Max(titleLabel.GetPreferredValues(titleLabel.text).x,
                descriptionLabel.GetPreferredValues(descriptionLabel.text).x) + padding.x * 2f;
            var width = Mathf.Min(availableWidth, Mathf.Clamp(preferredWidth, minWidth, maxWidth));
            var textWidth = Mathf.Max(1f, width - padding.x * 2f);
            var titleHeight = titleLabel.GetPreferredValues(titleLabel.text, textWidth, Mathf.Infinity).y;
            var descriptionHeight = descriptionLabel.GetPreferredValues(descriptionLabel.text, textWidth, Mathf.Infinity).y;

            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                padding.y * 2f + titleHeight + textSpacing + descriptionHeight);
            SetTextRect(titleLabel.rectTransform, textWidth, titleHeight, padding.y);
            SetTextRect(descriptionLabel.rectTransform, textWidth, descriptionHeight,
                padding.y + titleHeight + textSpacing);
        }

        private void SetTextRect(RectTransform rect, float width, float height, float top)
        {
            rect.anchoredPosition = new Vector2(padding.x, -top);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void UpdatePosition()
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, _screenPosition,
                    _eventCamera, out var position))
                return;

            var bounds = canvasRect.rect;
            position += cursorOffset;

            if (position.x + panel.rect.width > bounds.xMax - screenMargin)
                position.x -= panel.rect.width + cursorOffset.x * 2f;

            position.x = Mathf.Clamp(position.x, bounds.xMin + screenMargin,
                Mathf.Max(bounds.xMin + screenMargin, bounds.xMax - panel.rect.width - screenMargin));
            position.y = Mathf.Clamp(position.y,
                Mathf.Min(bounds.yMax - screenMargin, bounds.yMin + panel.rect.height + screenMargin),
                bounds.yMax - screenMargin);
            panel.anchoredPosition = position - bounds.min;
        }
    }
}
