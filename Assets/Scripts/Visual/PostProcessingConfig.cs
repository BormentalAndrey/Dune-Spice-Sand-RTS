using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Dune.SpiceAndSand.Visual
{
    /// <summary>
    /// Post-processing configuration for Dune's desert aesthetic
    /// References: Arrakis visual style
    /// </summary>
    [CreateAssetMenu(fileName = "PostProcessingConfig", menuName = "Dune/Post Processing Config")]
    public class PostProcessingConfig : ScriptableObject
    {
        [Header("Bloom")]
        public float bloomIntensity = 0.5f;
        public float bloomThreshold = 0.8f;
        public Color bloomTint = new Color(1f, 0.8f, 0.5f);
        
        [Header("Color Grading")]
        public float temperature = 0.2f; // Warm for desert
        public float tint = 0.1f;
        public float contrast = 0.1f;
        public float saturation = 0.8f;
        public Color colorFilter = new Color(1f, 0.95f, 0.85f);
        
        [Header("Vignette")]
        public float vignetteIntensity = 0.3f;
        public float vignetteSmoothness = 0.5f;
        public Color vignetteColor = new Color(0.1f, 0.05f, 0f);
        
        [Header("Depth of Field")]
        public bool enableDepthOfField = false;
        public float focusDistance = 30f;
        public float aperture = 2f;
        
        [Header("Motion Blur")]
        public bool enableMotionBlur = false;
        public float motionBlurIntensity = 0.3f;
        
        [Header("Heat Distortion")]
        public bool enableHeatDistortion = true;
        public float heatDistortionIntensity = 0.2f;
        public float heatDistortionSpeed = 1f;
        
        [Header("Sandstorm Effect")]
        public Color sandstormTint = new Color(0.6f, 0.4f, 0.2f);
        public float sandstormIntensity = 0.5f;
        
        private VolumeProfile currentProfile;
        private Volume currentVolume;
        
        public void ApplyToCamera(Camera camera)
        {
            if (camera == null) return;
            
            // Get or add volume
            currentVolume = camera.GetComponent<Volume>();
            if (currentVolume == null)
            {
                currentVolume = camera.gameObject.AddComponent<Volume>();
            }
            
            // Create or get profile
            currentProfile = currentVolume.profile;
            if (currentProfile == null)
            {
                currentProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                currentVolume.profile = currentProfile;
            }
            
            // Apply bloom
            if (currentProfile.TryGet<Bloom>(out var bloom))
            {
                bloom.intensity.value = bloomIntensity;
                bloom.threshold.value = bloomThreshold;
                bloom.tint.value = bloomTint;
            }
            else
            {
                bloom = currentProfile.Add<Bloom>();
                bloom.intensity.value = bloomIntensity;
                bloom.threshold.value = bloomThreshold;
                bloom.tint.value = bloomTint;
            }
            
            // Apply color grading
            if (currentProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                colorAdjustments.temperature.value = temperature;
                colorAdjustments.tint.value = tint;
                colorAdjustments.contrast.value = contrast;
                colorAdjustments.saturation.value = saturation;
                colorAdjustments.colorFilter.value = colorFilter;
            }
            else
            {
                colorAdjustments = currentProfile.Add<ColorAdjustments>();
                colorAdjustments.temperature.value = temperature;
                colorAdjustments.tint.value = tint;
                colorAdjustments.contrast.value = contrast;
                colorAdjustments.saturation.value = saturation;
                colorAdjustments.colorFilter.value = colorFilter;
            }
            
            // Apply vignette
            if (currentProfile.TryGet<Vignette>(out var vignette))
            {
                vignette.intensity.value = vignetteIntensity;
                vignette.smoothness.value = vignetteSmoothness;
                vignette.color.value = vignetteColor;
            }
            else
            {
                vignette = currentProfile.Add<Vignette>();
                vignette.intensity.value = vignetteIntensity;
                vignette.smoothness.value = vignetteSmoothness;
                vignette.color.value = vignetteColor;
            }
            
            // Apply depth of field
            if (enableDepthOfField)
            {
                if (currentProfile.TryGet<DepthOfField>(out var dof))
                {
                    dof.focusDistance.value = focusDistance;
                    dof.aperture.value = aperture;
                }
                else
                {
                    dof = currentProfile.Add<DepthOfField>();
                    dof.focusDistance.value = focusDistance;
                    dof.aperture.value = aperture;
                }
            }
            else if (currentProfile.TryGet<DepthOfField>(out var existingDOF))
            {
                currentProfile.Remove<DepthOfField>();
            }
            
            // Apply motion blur
            if (enableMotionBlur)
            {
                if (currentProfile.TryGet<MotionBlur>(out var motionBlur))
                {
                    motionBlur.intensity.value = motionBlurIntensity;
                }
                else
                {
                    motionBlur = currentProfile.Add<MotionBlur>();
                    motionBlur.intensity.value = motionBlurIntensity;
                }
            }
            else if (currentProfile.TryGet<MotionBlur>(out var existingMB))
            {
                currentProfile.Remove<MotionBlur>();
            }
        }
        
        public void SetSandstormMode(bool active)
        {
            if (currentProfile == null) return;
            
            if (active)
            {
                // Apply sandstorm tint
                if (currentProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.colorFilter.value = sandstormTint;
                }
                
                // Increase vignette
                if (currentProfile.TryGet<Vignette>(out var vignette))
                {
                    vignette.intensity.value = vignetteIntensity + 0.2f;
                }
            }
            else
            {
                // Restore normal colors
                if (currentProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.colorFilter.value = colorFilter;
                }
                
                // Restore vignette
                if (currentProfile.TryGet<Vignette>(out var vignette))
                {
                    vignette.intensity.value = vignetteIntensity;
                }
            }
        }
        
        public void SetPrescienceMode(bool active)
        {
            if (currentProfile == null) return;
            
            if (active)
            {
                // Blue tint for prescience
                if (currentProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.colorFilter.value = new Color(0.7f, 0.8f, 1f);
                    colorAdjustments.saturation.value = 0.9f;
                }
                
                // Add chromatic aberration
                if (!currentProfile.TryGet<ChromaticAberration>(out var _))
                {
                    var chromatic = currentProfile.Add<ChromaticAberration>();
                    chromatic.intensity.value = 0.3f;
                }
            }
            else
            {
                // Restore normal
                if (currentProfile.TryGet<ColorAdjustments>(out var colorAdjustments))
                {
                    colorAdjustments.colorFilter.value = colorFilter;
                    colorAdjustments.saturation.value = saturation;
                }
                
                // Remove chromatic aberration
                if (currentProfile.TryGet<ChromaticAberration>(out var chromatic))
                {
                    currentProfile.Remove<ChromaticAberration>();
                }
            }
        }
        
        private static PostProcessingConfig _instance;
        public static PostProcessingConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PostProcessingConfig>("PostProcessingConfig");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<PostProcessingConfig>();
                    }
                }
                return _instance;
            }
        }
    }
}
