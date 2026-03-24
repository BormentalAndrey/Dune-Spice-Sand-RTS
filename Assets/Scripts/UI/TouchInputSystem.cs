using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Touch input system for Android RTS controls
    /// </summary>
    public class TouchInputSystem : MonoBehaviour
    {
        [Header("Camera")]
        public Camera mainCamera;
        public float panSpeed = 10f;
        public float zoomSpeed = 5f;
        public float minZoom = 10f;
        public float maxZoom = 50f;
        
        [Header("Selection")]
        public LayerMask selectableLayer;
        public Color selectionBoxColor = new Color(0f, 1f, 0f, 0.3f);
        
        private Vector2 touchStartPos;
        private bool isDragging = false;
        private Rect selectionRect;
        
        private List<UnitBase> selectedUnits = new List<UnitBase>();
        
        private void Update()
        {
            HandleTouchInput();
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStartPos = touch.position;
                        isDragging = false;
                        break;
                        
                    case TouchPhase.Moved:
                        if (Vector2.Distance(touch.position, touchStartPos) > UIManager.Instance.dragThreshold)
                        {
                            isDragging = true;
                            UpdateSelectionBox(touchStartPos, touch.position);
                        }
                        break;
                        
                    case TouchPhase.Ended:
                        if (isDragging)
                        {
                            EndSelectionBox();
                        }
                        else
                        {
                            HandleTap(touch.position);
                        }
                        isDragging = false;
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                HandlePinchZoom();
            }
        }
        
        private void HandleTap(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectableLayer))
            {
                UnitBase unit = hit.collider.GetComponent<UnitBase>();
                if (unit != null)
                {
                    ClearSelection();
                    AddToSelection(unit);
                    UIManager.Instance.ShowUnitSelection(unit);
                }
                else
                {
                    // Move selected units to position
                    MoveSelectedUnits(hit.point);
                }
            }
            else
            {
                ClearSelection();
            }
        }
        
        private void UpdateSelectionBox(Vector2 start, Vector2 end)
        {
            selectionRect = new Rect(
                Mathf.Min(start.x, end.x),
                Mathf.Min(Screen.height - start.y, Screen.height - end.y),
                Mathf.Abs(start.x - end.x),
                Mathf.Abs(start.y - end.y)
            );
            
            // Draw selection box
            DrawSelectionBox();
        }
        
        private void DrawSelectionBox()
        {
            // Unity's GUI for debugging - in production, use UI Image
        }
        
        private void EndSelectionBox()
        {
            // Check all units within selection box
            Bounds selectionBounds = GetSelectionBounds();
            
            Collider[] colliders = Physics.OverlapBox(
                selectionBounds.center,
                selectionBounds.extents,
                Quaternion.identity,
                selectableLayer
            );
            
            ClearSelection();
            foreach (var collider in colliders)
            {
                UnitBase unit = collider.GetComponent<UnitBase>();
                if (unit != null && unit.faction == GameManager.Instance.playerFaction)
                {
                    AddToSelection(unit);
                }
            }
            
            if (selectedUnits.Count > 0)
            {
                UIManager.Instance.ShowUnitSelection(selectedUnits[0]);
            }
        }
        
        private Bounds GetSelectionBounds()
        {
            Vector3 min = mainCamera.ScreenToWorldPoint(new Vector3(selectionRect.xMin, selectionRect.yMin, 10f));
            Vector3 max = mainCamera.ScreenToWorldPoint(new Vector3(selectionRect.xMax, selectionRect.yMax, 10f));
            
            return new Bounds((min + max) / 2f, max - min);
        }
        
        private void HandlePinchZoom()
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            
            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            
            float prevMagnitude = (prevPos0 - prevPos1).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            
            float delta = currentMagnitude - prevMagnitude;
            
            Camera cam = mainCamera;
            float newSize = cam.orthographicSize - delta * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
        
        private void AddToSelection(UnitBase unit)
        {
            if (!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                unit.OnSelected();
            }
        }
        
        private void ClearSelection()
        {
            foreach (var unit in selectedUnits)
            {
                unit.OnDeselected();
            }
            selectedUnits.Clear();
            UIManager.Instance.HideUnitSelection();
        }
        
        private void MoveSelectedUnits(Vector3 targetPosition)
        {
            foreach (var unit in selectedUnits)
            {
                unit.MoveTo(targetPosition);
            }
        }
        
        private void HandleLongPress()
        {
            // Long press for context menu (Voice command, abilities)
        }
    }
}
