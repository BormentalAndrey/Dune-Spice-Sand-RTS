using UnityEngine;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Mobile-optimized camera controls
    /// Supports pan, zoom, rotation for RTS gameplay
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        public Camera mainCamera;
        public float panSpeed = 20f;
        public float zoomSpeed = 10f;
        public float rotationSpeed = 100f;
        
        [Header("Bounds")]
        public Bounds worldBounds;
        public float minZoom = 10f;
        public float maxZoom = 50f;
        public float currentZoom = 30f;
        
        [Header("Edge Scrolling")]
        public bool enableEdgeScrolling = true;
        public float edgeScrollThreshold = 50f;
        
        [Header("Touch Input")]
        public float panThreshold = 10f;
        public float rotationThreshold = 20f;
        
        private Vector3 lastPanPosition;
        private bool isPanning = false;
        private bool isRotating = false;
        private float targetZoom;
        
        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            targetZoom = currentZoom;
            UpdateCameraZoom();
        }
        
        private void Update()
        {
            HandleInput();
            UpdateCameraPosition();
            UpdateCameraZoom();
        }
        
        private void HandleInput()
        {
            // Touch input for mobile
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        lastPanPosition = touch.position;
                        isPanning = false;
                        isRotating = false;
                        break;
                        
                    case TouchPhase.Moved:
                        Vector2 delta = touch.position - (Vector2)lastPanPosition;
                        
                        // Check if panning or rotating based on movement
                        if (!isPanning && !isRotating)
                        {
                            if (Mathf.Abs(delta.x) > rotationThreshold)
                                isRotating = true;
                            else if (Mathf.Abs(delta.y) > panThreshold)
                                isPanning = true;
                        }
                        
                        if (isPanning)
                        {
                            PanCamera(delta);
                        }
                        else if (isRotating)
                        {
                            RotateCamera(delta.x);
                        }
                        
                        lastPanPosition = touch.position;
                        break;
                        
                    case TouchPhase.Ended:
                        isPanning = false;
                        isRotating = false;
                        break;
                }
            }
            else if (Input.touchCount == 2)
            {
                HandlePinchZoom();
            }
            
            // Edge scrolling for debugging (optional)
            if (enableEdgeScrolling && !isPanning)
            {
                HandleEdgeScrolling();
            }
            
            // Mouse input for editor testing
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                RotateCamera(mouseX * rotationSpeed * Time.deltaTime);
            }
            
            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                targetZoom -= scroll * zoomSpeed * 10f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
        
        private void PanCamera(Vector2 delta)
        {
            // Calculate pan movement based on camera orientation
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            
            // Remove vertical component
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            // Scale by screen size
            float panScale = panSpeed * (currentZoom / minZoom) * Time.deltaTime;
            Vector3 movement = (right * -delta.x + forward * -delta.y) * panScale;
            
            Vector3 newPosition = transform.position + movement;
            newPosition = ClampToBounds(newPosition);
            transform.position = newPosition;
        }
        
        private void RotateCamera(float deltaX)
        {
            float rotation = deltaX * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotation, Space.World);
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
            targetZoom -= delta * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        
        private void HandleEdgeScrolling()
        {
            Vector3 movement = Vector3.zero;
            
            // Screen edges
            if (Input.mousePosition.x >= Screen.width - edgeScrollThreshold)
                movement.x += 1;
            if (Input.mousePosition.x <= edgeScrollThreshold)
                movement.x -= 1;
            if (Input.mousePosition.y >= Screen.height - edgeScrollThreshold)
                movement.z += 1;
            if (Input.mousePosition.y <= edgeScrollThreshold)
                movement.z -= 1;
                
            if (movement != Vector3.zero)
            {
                movement = movement.normalized;
                movement = transform.TransformDirection(movement);
                movement.y = 0;
                
                Vector3 newPosition = transform.position + movement * panSpeed * Time.deltaTime;
                newPosition = ClampToBounds(newPosition);
                transform.position = newPosition;
            }
        }
        
        private void UpdateCameraZoom()
        {
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);
            
            // Position camera at correct height
            Vector3 pos = transform.position;
            pos.y = currentZoom;
            transform.position = pos;
            
            // Adjust orthographic size for ortho camera
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = currentZoom;
            }
        }
        
        private Vector3 ClampToBounds(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, worldBounds.min.x, worldBounds.max.x);
            position.z = Mathf.Clamp(position.z, worldBounds.min.z, worldBounds.max.z);
            return position;
        }
        
        public void FocusOnPosition(Vector3 position)
        {
            Vector3 newPosition = position;
            newPosition.y = transform.position.y;
            transform.position = ClampToBounds(newPosition);
        }
        
        public void FocusOnSelection(Vector3 center, float radius)
        {
            FocusOnPosition(center);
            
            // Adjust zoom to fit selection
            float targetZoomValue = radius * 2f;
            targetZoom = Mathf.Clamp(targetZoomValue, minZoom, maxZoom);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
        }
    }
}
