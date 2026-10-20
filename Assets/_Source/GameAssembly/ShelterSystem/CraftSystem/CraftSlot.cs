using System;
using GameAssembly.ItemsSystem;
using UnityEngine;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftSlot : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private ItemDataSO acceptedItem;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private float snapRadius = 0.35f;

        [Header("Visuals (Optional)")]
        [SerializeField] private GameObject ghostVisual;
        [SerializeField] private GameObject highlightVisual;

        public event Action<CraftSlot, CraftDraggableItem> OnItemPlaced;
        public event Action<CraftSlot> OnItemRemoved;

        public ItemDataSO AcceptedItem => acceptedItem;
        public float SnapRadius => snapRadius;
        public Vector3 SnapPosition => snapPoint != null ? snapPoint.position : transform.position;
        public Quaternion SnapRotation => snapPoint != null ? snapPoint.rotation : transform.rotation;

        public CraftDraggableItem CurrentItem { get; private set; }
        public bool IsOccupied => CurrentItem != null;
        public bool IsCorrectlyFilled => CurrentItem != null && CurrentItem.ItemData == acceptedItem;

        public void Initialize(ItemDataSO item, Transform point, float radius)
        {
            acceptedItem = item;
            snapPoint = point;
            snapRadius = radius;
            ResetSlot();
        }

        private void Awake()
        {
            if (snapPoint == null)
                snapPoint = transform;

            SetHighlight(false);
            UpdateGhostVisual();
        }

        public bool CanAccept(CraftDraggableItem item)
        {
            if (item == null)
                return false;

            if (IsOccupied && CurrentItem != item)
                return false;

            return item.ItemData == acceptedItem;
        }

        public void Place(CraftDraggableItem item)
        {
            CurrentItem = item;
            SetHighlight(false);
            UpdateGhostVisual();
            OnItemPlaced?.Invoke(this, item);
        }

        public void Clear()
        {
            CurrentItem = null;
            SetHighlight(false);
            UpdateGhostVisual();
            OnItemRemoved?.Invoke(this);
        }

        public void SetHighlight(bool active)
        {
            if (highlightVisual != null)
                highlightVisual.SetActive(active);
        }

        public void ResetSlot()
        {
            CurrentItem = null;
            SetHighlight(false);
            UpdateGhostVisual();
        }

        private void UpdateGhostVisual()
        {
            if (ghostVisual)
                ghostVisual.SetActive(!IsOccupied);
        }

        private void OnDrawGizmos()
        {
            var center = snapPoint ? snapPoint.position : transform.position;
            Gizmos.color = IsCorrectlyFilled ? Color.green : IsOccupied ? Color.yellow : Color.cyan;
            Gizmos.DrawWireSphere(center, snapRadius);

#if UNITY_EDITOR
            if (acceptedItem != null)
            {
                UnityEditor.Handles.Label(center + Vector3.up * (snapRadius + 0.05f), acceptedItem.ItemName);
            }
#endif
        }
    }
}
