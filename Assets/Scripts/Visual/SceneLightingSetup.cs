using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Visual
{
    /// <summary>
    /// Scene lighting setup for Arrakis day/night cycle
    /// References: Dune, Book I, Chapter 4 - Arrakis climate
    /// </summary>
    [ExecuteAlways]
    public class SceneLightingSetup : MonoBehaviour
    {
        [Header("Light Sources")]
        public Light sunLight;
        public Light ambientLight;
        
        [Header("Skybox")]
        public Material daySkybox;
        public Material nightSkybox;
        public Material sandstormSkybox;
        
        [Header("Color Gradients")]
        public Gradient skyColorGradient;
        public Gradient sunColorGradient;
        public Gradient ambientColorGradient;
        
        [Header("Intensity Curves")]
        public AnimationCurve sunIntensityCurve;
        public AnimationCurve ambientIntensityCurve;
        
        [Header("Sandstorm Effects")]
        public Color sandstormSkyColor = new Color(0.6f, 0.4f, 0.2f);
        public float sandstormSunIntensity = 0.3f;
        public float sandstormAmbientIntensity = 0.2f;
        
        [Header("Post Processing")]
        public Material postProcessMaterial;
        public float heatDistortionIntensity = 0.5f;
        
        private bool isSandstormActive = false;
        private float currentTimeOfDay = 0.5f; // 0-1, 0=midnight, 0.5=noon, 1=midnight
        
        private void Start()
        {
            if (sunLight == null)
                sunLight = GetComponent<Light>();
                
            if (sunLight == null)
                sunLight = FindObjectOfType<Light>();
                
            StartCoroutine(UpdateLighting());
        }
        
        private IEnumerator UpdateLighting()
        {
            while (true)
            {
                if (!isSandstormActive && TimeManager.Instance != null)
                {
                    currentTimeOfDay = TimeManager.Instance.currentHour / 24f;
                }
                
                UpdateSunPosition();
                UpdateLightColors();
                UpdateSkybox();
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void UpdateSunPosition()
        {
            if (sunLight == null) return;
            
            // Sun orbit around Y axis
            float sunAngle = currentTimeOfDay * 360f - 90f;
            float sunHeight = Mathf.Sin(currentTimeOfDay * Mathf.PI);
            
            // Position sun based on time
            Vector3 sunDirection = new Vector3(
                Mathf.Cos(sunAngle * Mathf.Deg2Rad),
                Mathf.Max(sunHeight, -0.2f),
                Mathf.Sin(sunAngle * Mathf.Deg2Rad)
            ).normalized;
            
            sunLight.transform.rotation = Quaternion.LookRotation(sunDirection);
            
            // Set intensity
            float intensity = sunIntensityCurve.Evaluate(currentTimeOfDay);
            if (isSandstormActive) intensity *= sandstormSunIntensity;
            sunLight.intensity = intensity;
        }
        
        private void UpdateLightColors()
        {
            if (sunLight == null) return;
            
            // Sun color based on time of day
            Color sunColor = sunColorGradient.Evaluate(currentTimeOfDay);
            if (isSandstormActive)
            {
                sunColor = Color.Lerp(sunColor, sandstormSkyColor, 0.7f);
            }
            sunLight.color = sunColor;
            
            // Ambient light
            if (ambientLight != null)
            {
                Color ambientColor = ambientColorGradient.Evaluate(currentTimeOfDay);
                float ambientIntensity = ambientIntensityCurve.Evaluate(currentTimeOfDay);
                if (isSandstormActive) ambientIntensity *= sandstormAmbientIntensity;
                
                ambientLight.color = ambientColor;
                ambientLight.intensity = ambientIntensity;
            }
            
            // Set ambient mode
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColorGradient.Evaluate(currentTimeOfDay) * 
                                          ambientIntensityCurve.Evaluate(currentTimeOfDay);
        }
        
        private void UpdateSkybox()
        {
            Material currentSkybox = daySkybox;
            
            if (isSandstormActive && sandstormSkybox != null)
            {
                currentSkybox = sandstormSkybox;
            }
            else if (currentTimeOfDay < 0.25f || currentTimeOfDay > 0.75f)
            {
                if (nightSkybox != null) currentSkybox = nightSkybox;
            }
            
            if (RenderSettings.skybox != currentSkybox)
            {
                RenderSettings.skybox = currentSkybox;
                DynamicGI.UpdateEnvironment();
            }
            
            // Update skybox color based on time
            if (currentSkybox != null && currentSkybox.HasProperty("_Tint"))
            {
                Color skyColor = skyColorGradient.Evaluate(currentTimeOfDay);
                if (isSandstormActive) skyColor = sandstormSkyColor;
                currentSkybox.SetColor("_Tint", skyColor);
            }
        }
        
        public void SetSandstormActive(bool active)
        {
            isSandstormActive = active;
            
            if (active)
            {
                // Reduce visibility
                RenderSettings.fog = true;
                RenderSettings.fogColor = sandstormSkyColor;
                RenderSettings.fogDensity = 0.05f;
            }
            else
            {
                RenderSettings.fog = false;
            }
        }
        
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (postProcessMaterial == null || isSandstormActive == false)
            {
                Graphics.Blit(source, destination);
                return;
            }
            
            // Apply heat distortion effect for desert
            float heatIntensity = Mathf.Sin(Time.time * 5f) * heatDistortionIntensity;
            postProcessMaterial.SetFloat("_HeatIntensity", heatIntensity);
            Graphics.Blit(source, destination, postProcessMaterial);
        }
        
        public void SetTimeOfDay(float time)
        {
            currentTimeOfDay = Mathf.Clamp01(time);
        }
        
        public float GetSunIntensity()
        {
            return sunLight != null ? sunLight.intensity : 0f;
        }
        
        public bool IsNightTime()
        {
            return currentTimeOfDay < 0.25f || currentTimeOfDay > 0.75f;
        }
    }
}
