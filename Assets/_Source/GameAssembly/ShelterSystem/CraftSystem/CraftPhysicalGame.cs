using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using PlayerInput = GameAssembly.Player.Input.PlayerInput;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftPhysicalGame : MonoBehaviour
    {
        [Header("Workspaces & Table")]
        [SerializeField] private Transform workspacesParent;
        [SerializeField] private List<CraftWorkspace> preconfiguredWorkspaces = new();
        [SerializeField] private Transform tableSurfaceTransform;
        [SerializeField] private Transform[] itemSpawnPoints;
        [SerializeField] private float dragElevation = 0.08f;
        [SerializeField] private LayerMask draggableLayers = ~0;
        [SerializeField] private bool returnToSpawnIfNotSnapped = false;

        [Header("Camera & Transitions")]
        [SerializeField] private Camera interactionCamera;
        [SerializeField] private float cameraTransitionDuration = 0.5f;

        [Header("UI Feedback")]
        [SerializeField] private CraftSuccessUI successUI;

        [Inject] private ShelterInventory _shelterInventory;
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private PlayerInput _playerInput;

        private readonly Dictionary<CraftItemSO, CraftWorkspace> _workspaceMap = new();
        private CraftWorkspace _currentWorkspace;
        private CraftItemSO _currentCraft;
        private CraftDraggableItem _draggedItem;
        private CraftSlot _hoveredSlot;
        private Plane _dragPlane;
        private bool _isCrafting;
        private bool _isCompleting;

        private Action _onFinishedCallback;
        private Action _onCancelledCallback;

        public bool IsCrafting => _isCrafting;

        private void Awake()
        {
            RegisterWorkspaces();

            if (successUI == null)
            {
                successUI = FindAnyObjectByType<CraftSuccessUI>();
                if (successUI == null)
                {
                    successUI = gameObject.AddComponent<CraftSuccessUI>();
                }
            }

            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }
        }

        public void RegisterWorkspaces()
        {
            _workspaceMap.Clear();

            // Find in preconfigured list
            foreach (var ws in preconfiguredWorkspaces)
            {
                if (ws != null && ws.CraftItem != null && !_workspaceMap.ContainsKey(ws.CraftItem))
                {
                    _workspaceMap.Add(ws.CraftItem, ws);
                    ws.gameObject.SetActive(false);
                }
            }

            // Find in workspacesParent if assigned
            if (workspacesParent != null)
            {
                var children = workspacesParent.GetComponentsInChildren<CraftWorkspace>(true);
                foreach (var ws in children)
                {
                    if (ws != null && ws.CraftItem != null && !_workspaceMap.ContainsKey(ws.CraftItem))
                    {
                        _workspaceMap.Add(ws.CraftItem, ws);
                        ws.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void StartCraft(CraftItemSO craft, Action onFinished, Action onCancelled = null)
        {
            if (craft == null)
            {
                Debug.LogError("CraftItemSO is null!");
                return;
            }

            _onFinishedCallback = onFinished;
            _onCancelledCallback = onCancelled;
            _currentCraft = craft;

            // Find matching workspace
            if (!_workspaceMap.TryGetValue(craft, out var workspace) || workspace == null)
            {
                // Fallback: search in scene
                workspace = FindObjectsByType<CraftWorkspace>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(w => w.CraftItem == craft);
            }

            if (workspace == null)
            {
                Debug.LogError($"[CraftPhysicalGame] Assign a CraftWorkspace for {craft.name}!");
                _currentCraft = null;
                _onCancelledCallback?.Invoke();
                return;
            }

            _currentWorkspace = workspace;
            if (!_currentWorkspace.SetupWorkspace(itemSpawnPoints))
            {
                _currentWorkspace = null;
                _currentCraft = null;
                _onCancelledCallback?.Invoke();
                return;
            }

            SetupDragPlane();
            TransitionCameraToWorkspace();

            _isCrafting = true;
            _isCompleting = false;
        }

        public void CancelCraft()
        {
            if (!_isCrafting || _isCompleting)
                return;

            _isCrafting = false;
            _draggedItem = null;

            if (_hoveredSlot != null)
            {
                _hoveredSlot.SetHighlight(false);
                _hoveredSlot = null;
            }

            if (_currentWorkspace != null)
            {
                _currentWorkspace.ClearWorkspace();
            }

            _onCancelledCallback?.Invoke();
        }

        private void Update()
        {
            if (!_isCrafting || _isCompleting || _playerState.IsMenuOpen())
                return;

            // Handle cancellation via Escape
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelCraft();
                return;
            }

            HandleMouseInteraction();
        }

        private void HandleMouseInteraction()
        {
            Camera cam = GetActiveCamera();
            if (cam == null) return;

            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(mouseScreenPos);

            bool isMouseDown = Mouse.current != null ? Mouse.current.leftButton.wasPressedThisFrame : Input.GetMouseButtonDown(0);
            bool isMouseHeld = Mouse.current != null ? Mouse.current.leftButton.isPressed : Input.GetMouseButton(0);
            bool isMouseUp = Mouse.current != null ? Mouse.current.leftButton.wasReleasedThisFrame : Input.GetMouseButtonUp(0);

            // 1. Mouse Down - Pick up item
            if (isMouseDown)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 50f, draggableLayers))
                {
                    var draggable = hit.collider.GetComponentInParent<CraftDraggableItem>();
                    if (draggable && _currentWorkspace.ActiveItems.Contains(draggable))
                    {
                        _draggedItem = draggable;
                        _draggedItem.OnStartDrag();

                        // Set the drag plane dynamically at the exact height of the item on the table
                        float itemHeight = _draggedItem.transform.position.y;
                        _dragPlane = new Plane(Vector3.up, new Vector3(0f, itemHeight + dragElevation, 0f));
                    }
                }
            }

            // 2. Mouse Held - Drag item & detect hover over slots
            if (isMouseHeld && _draggedItem != null)
            {
                if (_dragPlane.Raycast(ray, out float enterDist))
                {
                    Vector3 worldPos = ray.GetPoint(enterDist);
                    _draggedItem.UpdateDragPosition(worldPos);
                }

                // Slot hover detection
                CraftSlot nearestSlot = FindClosestAcceptingSlot(_draggedItem);
                if (nearestSlot != _hoveredSlot)
                {
                    if (_hoveredSlot != null)
                        _hoveredSlot.SetHighlight(false);

                    _hoveredSlot = nearestSlot;

                    if (_hoveredSlot != null)
                        _hoveredSlot.SetHighlight(true);
                }
            }

            // 3. Mouse Up - Snap or drop item
            if (isMouseUp && _draggedItem != null)
            {
                CraftSlot targetSlot = FindClosestAcceptingSlot(_draggedItem);

                if (_hoveredSlot != null)
                {
                    _hoveredSlot.SetHighlight(false);
                    _hoveredSlot = null;
                }

                if (targetSlot != null)
                {
                    _draggedItem.SnapToSlot(targetSlot, smooth: true);
                    CheckCompletion();
                }
                else
                {
                    if (returnToSpawnIfNotSnapped)
                    {
                        _draggedItem.ReturnToSpawn(smooth: true);
                    }
                    else
                    {
                        _draggedItem.DropOnTable(_draggedItem.transform.position, smooth: true);
                    }
                }

                _draggedItem = null;
            }
        }

        private CraftSlot FindClosestAcceptingSlot(CraftDraggableItem item)
        {
            if (_currentWorkspace == null || _currentWorkspace.Slots == null || item == null)
                return null;

            CraftSlot closestSlot = null;
            float minDistance = float.MaxValue;

            foreach (var slot in _currentWorkspace.Slots)
            {
                if (slot == null || !slot.CanAccept(item))
                    continue;

                float dist = Vector3.Distance(item.transform.position, slot.SnapPosition);
                if (dist <= slot.SnapRadius && dist < minDistance)
                {
                    minDistance = dist;
                    closestSlot = slot;
                }
            }

            return closestSlot;
        }

        private void CheckCompletion()
        {
            if (_currentWorkspace == null || !_currentWorkspace.IsCompleted)
                return;

            OnCraftSuccess();
        }

        private void OnCraftSuccess()
        {
            _isCompleting = true;
            _isCrafting = false;

            if (_hoveredSlot != null)
            {
                _hoveredSlot.SetHighlight(false);
                _hoveredSlot = null;
            }

            // Delay slightly for smooth final snap
            DOVirtual.DelayedCall(0.3f, () =>
            {
                string itemName = _currentCraft != null && _currentCraft.ItemData != null 
                    ? _currentCraft.ItemData.ItemName 
                    : "ПРЕДМЕТ";

                successUI.PlaySuccessAnimation(itemName, FinalizeCraft);
            });
        }

        private void FinalizeCraft()
        {
            // Deduct required ingredients from shelter inventory
            if (_shelterInventory != null && _currentCraft != null && _currentCraft.CraftsRequirements != null)
            {
                foreach (var req in _currentCraft.CraftsRequirements)
                {
                    _shelterInventory.TryRemoveItem(req.ItemData, req.Count);
                }

                _shelterInventory.AddItem(_currentCraft.ItemData, 1);
            }

            // Clean up workspace
            if (_currentWorkspace != null)
            {
                _currentWorkspace.ClearWorkspace();
                _currentWorkspace = null;
            }

            // Reset camera & restore player state
            if (_playerCamera != null)
            {
                _playerCamera.ResetCameraToStartPosition();
            }

            if (_playerState != null)
            {
                _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
            }

            _isCompleting = false;
            _onFinishedCallback?.Invoke();
        }

        private void SetupDragPlane()
        {
            float tableY = 0f;

            if (tableSurfaceTransform != null)
            {
                if (tableSurfaceTransform.TryGetComponent(out Collider col))
                    tableY = col.bounds.max.y;
                else
                    tableY = tableSurfaceTransform.position.y;
            }
            else if (_currentWorkspace != null)
            {
                if (_currentWorkspace.Slots != null && _currentWorkspace.Slots.Count > 0 && _currentWorkspace.Slots[0] != null)
                    tableY = _currentWorkspace.Slots[0].SnapPosition.y;
                else if (_currentWorkspace.ActiveItems != null && _currentWorkspace.ActiveItems.Count > 0 && _currentWorkspace.ActiveItems[0] != null)
                    tableY = _currentWorkspace.ActiveItems[0].transform.position.y;
                else
                    tableY = _currentWorkspace.transform.position.y;
            }
            else
            {
                tableY = transform.position.y;
            }

            _dragPlane = new Plane(Vector3.up, new Vector3(0f, tableY + dragElevation, 0f));
        }

        private void TransitionCameraToWorkspace()
        {
            if (_currentWorkspace == null || _playerCamera == null)
                return;

            Transform targetCam = _currentWorkspace.CloseUpCameraPoint;
            if (targetCam != null)
            {
                _playerCamera.transform.DOMove(targetCam.position, cameraTransitionDuration).SetEase(Ease.OutQuad);
                _playerCamera.transform.DORotateQuaternion(targetCam.rotation, cameraTransitionDuration).SetEase(Ease.OutQuad);
            }
        }

        private Camera GetActiveCamera()
        {
            if (interactionCamera != null && interactionCamera.gameObject.activeInHierarchy)
                return interactionCamera;

            return Camera.main;
        }

    }
}
