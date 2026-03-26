using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Input
{
    /// <summary>
    /// Touch input configuration for mobile RTS controls
    /// References: Mobile-optimized Dune gameplay
    /// </summary>
    [CreateAssetMenu(fileName = "InputConfig", menuName = "Dune/Input Configuration")]
    public class InputConfig : ScriptableObject
    {
        [Header("Touch Settings")]
        public float tapTimeThreshold = 0.3f;
        public float tapDistanceThreshold = 20f;
        public float longPressTime = 0.5f;
        public float doubleTapTime = 0.3f;
        
        [Header("Gesture Settings")]
        public float dragThreshold = 20f;
        public float pinchThreshold = 20f;
        public float rotationThreshold = 30f;
        
        [Header("Selection Settings")]
        public float selectionRadius = 50f;
        public bool enableAreaSelection = true;
        public bool enableBoxSelection = true;
        
        [Header("Camera Settings")]
        public float edgeScrollThreshold = 50f;
        public float cameraPanSpeed = 20f;
        public float cameraZoomSpeed = 10f;
        public float cameraRotationSpeed = 100f;
        
        [Header("Haptic Feedback")]
        public bool enableHapticFeedback = true;
        public float selectionHapticIntensity = 0.2f;
        public float combatHapticIntensity = 0.5f;
        
        private static InputConfig _instance;
        public static InputConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<InputConfig>("InputConfig");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<InputConfig>();
                        Debug.LogWarning("InputConfig not found, using defaults");
                    }
                }
                return _instance;
            }
        }
        
        public bool IsTap(Touch touch)
        {
            return touch.phase == TouchPhase.Ended && 
                   touch.deltaTime < tapTimeThreshold &&
                   touch.deltaPosition.magnitude < tapDistanceThreshold;
        }
        
        public bool IsLongPress(Touch touch)
        {
            return touch.phase == TouchPhase.Stationary &&
                   touch.deltaTime > longPressTime;
        }
        
        public bool IsDoubleTap(Touch touch, float lastTapTime)
        {
            return touch.phase == TouchPhase.Ended &&
                   Time.time - lastTapTime < doubleTapTime;
        }
        
        public bool IsDrag(Touch touch)
        {
            return touch.phase == TouchPhase.Moved &&
                   touch.deltaPosition.magnitude > dragThreshold;
        }
        
        public Vector2 GetPanDelta(Touch touch)
        {
            return touch.deltaPosition;
        }
        
        public float GetPinchDelta(Touch touch0, Touch touch1)
        {
            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            
            float prevMagnitude = (prevPos0 - prevPos1).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            
            return currentMagnitude - prevMagnitude;
        }
        
        public float GetRotationDelta(Touch touch0, Touch touch1)
        {
            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            
            Vector2 prevDirection = (prevPos0 - prevPos1).normalized;
            Vector2 currentDirection = (touch0.position - touch1.position).normalized;
            
            float angle = Vector2.SignedAngle(prevDirection, currentDirection);
            return angle;
        }
        
        public void TriggerHaptic(HapticType type)
        {
            if (!enableHapticFeedback) return;
            
            float intensity = type == HapticType.Selection ? selectionHapticIntensity : combatHapticIntensity;
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (intensity < 0.3f)
                Handheld.Vibrate();
            else
                Handheld.Vibrate();
            #endif
        }
        
        public enum HapticType
        {
            Selection,
            Combat,
            Building,
            Warning
        }
    }
}
