using UnityEngine;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Quality presets for different device performance tiers
    /// References: Mobile optimization for Dune gameplay
    /// </summary>
    [CreateAssetMenu(fileName = "QualityPresets", menuName = "Dune/Quality Presets")]
    public class QualityPresets : ScriptableObject
    {
        [Header("Device Tiers")]
        public QualityProfile lowEnd;
        public QualityProfile midRange;
        public QualityProfile highEnd;
        public QualityProfile ultra;
        
        [System.Serializable]
        public class QualityProfile
        {
            public string name;
            public int qualityLevel;
            
            [Header("Graphics")]
            public int shadowQuality = 1;
            public int textureQuality = 1;
            public int antiAliasing = 2;
            public float renderScale = 1f;
            public bool realtimeReflectionProbes = false;
            
            [Header("Rendering")]
            public int pixelLightCount = 2;
            public float shadowDistance = 50f;
            public int shadowCascades = 2;
            
            [Header("Post Processing")]
            public bool enableBloom = true;
            public bool enableDepthOfField = false;
            public bool enableMotionBlur = false;
            public bool enableVignette = true;
            
            [Header("Performance")]
            public int maxVisibleUnits = 50;
            public float lodDistance = 50f;
            public float cullingDistance = 100f;
            public bool enableObjectPooling = true;
            public bool enableDynamicResolution = true;
            
            [Header("Particles")]
            public int maxParticleCount = 1000;
            public float particleQuality = 0.5f;
            
            [Header("Audio")]
            public int audioChannels = 32;
            public float audioQuality = 0.8f;
        }
        
        private static QualityPresets _instance;
        public static QualityPresets Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<QualityPresets>("QualityPresets");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<QualityPresets>();
                        _instance.InitializeDefaults();
                    }
                }
                return _instance;
            }
        }
        
        private void InitializeDefaults()
        {
            lowEnd = new QualityProfile
            {
                name = "Low",
                qualityLevel = 0,
                shadowQuality = 0,
                textureQuality = 2,
                antiAliasing = 0,
                renderScale = 0.7f,
                maxVisibleUnits = 30,
                lodDistance = 30f,
                cullingDistance = 70f
            };
            
            midRange = new QualityProfile
            {
                name = "Medium",
                qualityLevel = 1,
                shadowQuality = 1,
                textureQuality = 1,
                antiAliasing = 2,
                renderScale = 1f,
                maxVisibleUnits = 50,
                lodDistance = 50f,
                cullingDistance = 100f
            };
            
            highEnd = new QualityProfile
            {
                name = "High",
                qualityLevel = 2,
                shadowQuality = 2,
                textureQuality = 0,
                antiAliasing = 4,
                renderScale = 1f,
                maxVisibleUnits = 80,
                lodDistance = 80f,
                cullingDistance = 150f
            };
            
            ultra = new QualityProfile
            {
                name = "Ultra",
                qualityLevel = 3,
                shadowQuality = 3,
                textureQuality = 0,
                antiAliasing = 8,
                renderScale = 1f,
                maxVisibleUnits = 100,
                lodDistance = 100f,
                cullingDistance = 200f
            };
        }
        
        public QualityProfile GetProfileForDevice()
        {
            // Detect device performance tier
            int processorCount = SystemInfo.processorCount;
            long memorySize = SystemInfo.systemMemorySize;
            
            if (processorCount <= 4 || memorySize <= 2048)
            {
                return lowEnd;
            }
            else if (processorCount <= 6 || memorySize <= 4096)
            {
                return midRange;
            }
            else if (processorCount <= 8 || memorySize <= 6144)
            {
                return highEnd;
            }
            else
            {
                return ultra;
            }
        }
        
        public void ApplyProfile(QualityProfile profile)
        {
            if (profile == null) return;
            
            // Apply quality level
            QualitySettings.SetQualityLevel(profile.qualityLevel);
            QualitySettings.shadowQuality = (ShadowQuality)profile.shadowQuality;
            QualitySettings.masterTextureLimit = profile.textureQuality;
            QualitySettings.antiAliasing = profile.antiAliasing;
            QualitySettings.pixelLightCount = profile.pixelLightCount;
            QualitySettings.shadowDistance = profile.shadowDistance;
            QualitySettings.shadowCascades = profile.shadowCascades;
            
            // Apply performance settings
            if (PerformanceOptimizer.Instance != null)
            {
                PerformanceOptimizer.Instance.maxVisibleUnits = profile.maxVisibleUnits;
                PerformanceOptimizer.Instance.lodDistance = profile.lodDistance;
                PerformanceOptimizer.Instance.cullingDistance = profile.cullingDistance;
            }
            
            // Apply render scale
            if (Camera.main != null && profile.renderScale < 1f)
            {
                // Dynamic resolution would be set here
            }
            
            Debug.Log($"Applied quality profile: {profile.name}");
        }
        
        public QualityProfile GetProfileByName(string name)
        {
            switch (name.ToLower())
            {
                case "low": return lowEnd;
                case "medium": return midRange;
                case "high": return highEnd;
                case "ultra": return ultra;
                default: return midRange;
            }
        }
    }
}
