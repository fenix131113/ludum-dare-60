using System;
using DG.Tweening;
using GameAssembly.ItemsSystem;
using UnityEngine;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    [RequireComponent(typeof(Collider))]
    public class CraftDraggableItem : MonoBehaviour
    {
        [Header("Item Configuration")]
        [SerializeField] private ItemDataSO itemData;
        [SerializeField] private Collider itemCollider;
        [SerializeField] private float snapDuration = 0.2f;

        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _baseScale;
        private Tween _moveTween;
        private Tween _rotateTween;
        private Tween _scaleTween;

        public ItemDataSO ItemData => itemData;
        public CraftSlot CurrentSlot { get; private set; }
        public bool IsDragging { get; private set; }
        public Collider Collider => itemCollider;

        private void Awake()
        {
            if (itemCollider == null)
                itemCollider = GetComponent<Collider>();

            _baseScale = transform.localScale;
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        public void Initialize(ItemDataSO data, Vector3 spawnPos, Quaternion spawnRot)
        {
            itemData = data;
            _spawnPosition = spawnPos;
            _spawnRotation = spawnRot;
            transform.position = spawnPos;
            transform.rotation = spawnRot;
            _baseScale = transform.localScale;
            CurrentSlot = null;
            IsDragging = false;
        }

        public void SetItemData(ItemDataSO data)
        {
            itemData = data;
        }

        public void SetSpawnTransform(Vector3 position, Quaternion rotation)
        {
            _spawnPosition = position;
            _spawnRotation = rotation;
        }

        public void OnStartDrag()
        {
            IsDragging = true;
            KillTweens();

            if (CurrentSlot != null)
            {
                CurrentSlot.Clear();
                CurrentSlot = null;
            }

            _scaleTween = transform.DOScale(_baseScale * 1.08f, 0.15f).SetEase(Ease.OutQuad);
        }

        public void UpdateDragPosition(Vector3 targetWorldPosition, float followSpeed = 25f)
        {
            transform.position = Vector3.Lerp(transform.position, targetWorldPosition, Time.deltaTime * followSpeed);
        }

        public void SnapToSlot(CraftSlot slot, bool smooth = true)
        {
            IsDragging = false;
            KillTweens();

            if (CurrentSlot != null && CurrentSlot != slot)
            {
                CurrentSlot.Clear();
            }

            CurrentSlot = slot;
            slot.Place(this);

            if (smooth)
            {
                _moveTween = transform.DOMove(slot.SnapPosition, snapDuration).SetEase(Ease.OutBack);
                _rotateTween = transform.DORotateQuaternion(slot.SnapRotation, snapDuration).SetEase(Ease.OutQuad);
                _scaleTween = transform.DOScale(_baseScale, snapDuration).SetEase(Ease.OutQuad);
            }
            else
            {
                transform.position = slot.SnapPosition;
                transform.rotation = slot.SnapRotation;
                transform.localScale = _baseScale;
            }
        }

        public void DropOnTable(Vector3 tablePosition, bool smooth = true)
        {
            IsDragging = false;
            KillTweens();

            if (smooth)
            {
                _moveTween = transform.DOMove(tablePosition, snapDuration).SetEase(Ease.OutQuad);
                _scaleTween = transform.DOScale(_baseScale, snapDuration).SetEase(Ease.OutQuad);
            }
            else
            {
                transform.position = tablePosition;
                transform.localScale = _baseScale;
            }
        }

        public void ReturnToSpawn(bool smooth = true)
        {
            IsDragging = false;
            KillTweens();

            if (CurrentSlot != null)
            {
                CurrentSlot.Clear();
                CurrentSlot = null;
            }

            if (smooth)
            {
                _moveTween = transform.DOMove(_spawnPosition, snapDuration).SetEase(Ease.OutQuad);
                _rotateTween = transform.DORotateQuaternion(_spawnRotation, snapDuration).SetEase(Ease.OutQuad);
                _scaleTween = transform.DOScale(_baseScale, snapDuration).SetEase(Ease.OutQuad);
            }
            else
            {
                transform.position = _spawnPosition;
                transform.rotation = _spawnRotation;
                transform.localScale = _baseScale;
            }
        }

        public void ResetItem()
        {
            ReturnToSpawn(smooth: false);
        }

        private void KillTweens()
        {
            _moveTween?.Kill();
            _rotateTween?.Kill();
            _scaleTween?.Kill();
        }

        private void OnDestroy()
        {
            KillTweens();
        }
    }
}
