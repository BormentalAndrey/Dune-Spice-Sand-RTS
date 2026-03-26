using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Platform detection and optimization for different devices
    /// References: Mobile performance for Dune gameplay
    /// </summary>
    public class PlatformDetector : MonoBehaviour
    {
        public static PlatformDetector Instance { get; private set; }
        
        [Header("Device Info")]
        public DeviceTier deviceTier = DeviceTier.MidRange;
        public float deviceScore = 0f;
        
        [Header("Performance Recommendations")]
        public int recommendedFPS = 60;
        public float recommendedRenderScale = 1f;
        public int recommendedShadowQuality = 1;
        
        public enum DeviceTier
        {
            Low,
            MidRange,
            High,
            Ultra
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            DetectDeviceCapabilities();
        }
        
        private void DetectDeviceCapabilities()
        {
            // Calculate device score based on specs
            deviceScore = 0;
            
            // Processor
            int processorCount = SystemInfo.processorCount;
            deviceScore += Mathf.Clamp(processorCount / 8f, 0, 1) * 0.3f;
            
            // Memory
            long memorySize = SystemInfo.systemMemorySize;
            deviceScore += Mathf.Clamp(memorySize / 8192f, 0, 1) * 0.3f;
            
            // Graphics
            int graphicsMemory = SystemInfo.graphicsMemorySize;
            deviceScore += Mathf.Clamp(graphicsMemory / 4096f, 0, 1) * 0.2f;
            
            // Graphics tier
            int graphicsTier = (int)SystemInfo.graphicsDeviceType;
            deviceScore += Mathf.Clamp(graphicsTier / 10f, 0, 1) * 0.2f;
            
            // Determine tier
            if (deviceScore < 0.3f)
            {
                deviceTier = DeviceTier.Low;
                recommendedFPS = 30;
                recommendedRenderScale = 0.7f;
                recommendedShadowQuality = 0;
            }
            else if (deviceScore < 0.6f)
            {
                deviceTier = DeviceTier.MidRange;
                recommendedFPS = 60;
                recommendedRenderScale = 1f;
                recommendedShadowQuality = 1;
            }
            else if (deviceScore < 0.8f)
            {
                deviceTier = DeviceTier.High;
                recommendedFPS = 60;
                recommendedRenderScale = 1f;
                recommendedShadowQuality = 2;
            }
            else
            {
                deviceTier = DeviceTier.Ultra;
                recommendedFPS = 60;
                recommendedRenderScale = 1f;
                recommendedShadowQuality = 3;
            }
            
            Debug.Log($"Device Detected: {SystemInfo.deviceModel}");
            Debug.Log($"Tier: {deviceTier}, Score: {deviceScore:F2}");
            Debug.Log($"Processor: {processorCount} cores");
            Debug.Log($"Memory: {memorySize} MB");
            Debug.Log($"Graphics: {SystemInfo.graphicsDeviceName}");
        }
        
        public void ApplyOptimizedSettings()
        {
            // Apply recommended settings
            Application.targetFrameRate = recommendedFPS;
            
            // Apply quality settings based on tier
            if (QualityPresets.Instance != null)
            {
                QualityPresets.QualityProfile profile;
                switch (deviceTier)
                {
                    case DeviceTier.Low:
                        profile = QualityPresets.Instance.lowEnd;
                        break;
                    case DeviceTier.MidRange:
                        profile = QualityPresets.Instance.midRange;
                        break;
                    case DeviceTier.High:
                        profile = QualityPresets.Instance.highEnd;
                        break;
                    default:
                        profile = QualityPresets.Instance.ultra;
                        break;
                }
                QualityPresets.Instance.ApplyProfile(profile);
            }
            
            // Apply shadow quality
            QualitySettings.shadowQuality = (ShadowQuality)recommendedShadowQuality;
            
            // Apply render scale
            if (recommendedRenderScale < 1f && ScalableBufferManager.IsDynamicResolutionSupported())
            {
                ScalableBufferManager.ResizeBuffers(recommendedRenderScale, recommendedRenderScale);
            }
            
            Debug.Log($"Applied optimized settings for {deviceTier} device");
        }
        
        public bool IsLowEndDevice()
        {
            return deviceTier == DeviceTier.Low;
        }
        
        public bool IsHighEndDevice()
        {
            return deviceTier == DeviceTier.High || deviceTier == DeviceTier.Ultra;
        }
        
        public bool SupportsVulkan()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan;
        }
        
        public bool SupportsComputeShaders()
        {
            return SystemInfo.supportsComputeShaders;
        }
        
        public int GetRecommendedParticleCount()
        {
            switch (deviceTier)
            {
                case DeviceTier.Low: return 500;
                case DeviceTier.MidRange: return 1000;
                case DeviceTier.High: return 2000;
                default: return 3000;
            }
        }
        
        public float GetRecommendedLODDistance()
        {
            switch (deviceTier)
            {
                case DeviceTier.Low: return 30f;
                case DeviceTier.MidRange: return 50f;
                case DeviceTier.High: return 80f;
                default: return 100f;
            }
        }
    }
}
