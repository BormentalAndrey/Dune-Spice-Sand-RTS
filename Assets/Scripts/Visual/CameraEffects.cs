using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Visual
{
    /// <summary>
    /// Camera effects for cinematic moments
    /// References: Dune cinematic moments - worm emergence, spice trance
    /// </summary>
    public class CameraEffects : MonoBehaviour
    {
        public static CameraEffects Instance { get; private set; }
        
        [Header("Camera")]
        public Camera mainCamera;
        
        [Header("Shake Settings")]
        public float defaultShakeDuration = 0.5f;
        public float defaultShakeMagnitude = 0.3f;
        
        [Header("FOV Effects")]
        public float defaultFOV = 60f;
        public float prescienceFOV = 75f;
        public float combatFOV = 65f;
        
        [Header("Color Effects")]
        public Color normalColor = Color.white;
        public Color prescienceColor = new Color(0.8f, 0.9f, 1f);
        public Color damageColor = new Color(1f, 0.5f, 0.5f);
        public Color spiceTranceColor = new Color(1f, 0.7f, 0.3f);
        
        [Header("Post Processing")]
        public Material postProcessMaterial;
        public float vignetteIntensity = 0.3f;
        
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private float originalFOV;
        private Coroutine currentEffect;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            if (mainCamera != null)
            {
                originalPosition = mainCamera.transform.localPosition;
                originalRotation = mainCamera.transform.localRotation;
                originalFOV = mainCamera.fieldOfView;
            }
        }
        
        public void ShakeCamera(float duration = -1f, float magnitude = -1f)
        {
            if (currentEffect != null) StopCoroutine(currentEffect);
            
            if (duration < 0) duration = defaultShakeDuration;
            if (magnitude < 0) magnitude = defaultShakeMagnitude;
            
            currentEffect = StartCoroutine(ShakeRoutine(duration, magnitude));
        }
        
        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0;
            Vector3 startPos = mainCamera.transform.localPosition;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float currentMag = magnitude * (1 - t);
                
                float x = Random.Range(-currentMag, currentMag);
                float y = Random.Range(-currentMag, currentMag);
                
                mainCamera.transform.localPosition = startPos + new Vector3(x, y, 0);
                
                yield return null;
            }
            
            mainCamera.transform.localPosition = originalPosition;
            currentEffect = null;
        }
        
        public void SetFOV(float targetFOV, float duration = 0.5f)
        {
            if (currentEffect != null) StopCoroutine(currentEffect);
            currentEffect = StartCoroutine(FOVRoutine(targetFOV, duration));
        }
        
        private IEnumerator FOVRoutine(float targetFOV, float duration)
        {
            float startFOV = mainCamera.fieldOfView;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
                yield return null;
            }
            
            mainCamera.fieldOfView = targetFOV;
            currentEffect = null;
        }
        
        public void ResetFOV(float duration = 0.5f)
        {
            SetFOV(originalFOV, duration);
        }
        
        public void ActivatePrescience()
        {
            SetFOV(prescienceFOV, 0.3f);
            StartCoroutine(ColorEffectRoutine(prescienceColor, 0.3f, 0.5f));
            StartCoroutine(ChromaticAberrationRoutine(0.5f, 0.3f));
        }
        
        public void DeactivatePrescience()
        {
            ResetFOV();
            StartCoroutine(ColorEffectRoutine(normalColor, 0.3f, 0.5f));
            StartCoroutine(ChromaticAberrationRoutine(0f, 0.3f));
        }
        
        public void ActivateSpiceTrance()
        {
            StartCoroutine(SpiceTranceRoutine());
        }
        
        private IEnumerator SpiceTranceRoutine()
        {
            float duration = 2f;
            float elapsed = 0;
            
            // Color distortion
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Oscillating color shift
                Color effectColor = Color.Lerp(spiceTranceColor, normalColor, t);
                postProcessMaterial.SetColor("_ColorShift", effectColor);
                
                // Bloom intensity
                float bloom = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
                postProcessMaterial.SetFloat("_BloomIntensity", bloom * (1 - t));
                
                yield return null;
            }
            
            postProcessMaterial.SetFloat("_BloomIntensity", 0);
        }
        
        public void ActivateWormApproach()
        {
            ShakeCamera(0.3f, 0.5f);
            StartCoroutine(RedFlashRoutine());
        }
        
        private IEnumerator RedFlashRoutine()
        {
            float duration = 0.2f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                postProcessMaterial.SetFloat("_VignetteIntensity", vignetteIntensity + t * 2f);
                postProcessMaterial.SetColor("_ColorTint", Color.Lerp(Color.white, Color.red, t));
                yield return null;
            }
            
            // Fade back
            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                postProcessMaterial.SetFloat("_VignetteIntensity", Mathf.Lerp(0.5f, vignetteIntensity, t));
                postProcessMaterial.SetColor("_ColorTint", Color.Lerp(Color.red, Color.white, t));
                yield return null;
            }
            
            postProcessMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
            postProcessMaterial.SetColor("_ColorTint", Color.white);
        }
        
        private IEnumerator ColorEffectRoutine(Color targetColor, float duration, float returnDuration)
        {
            Color startColor = postProcessMaterial.GetColor("_ColorTint");
            float elapsed = 0;
            
            // Fade to target
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                postProcessMaterial.SetColor("_ColorTint", Color.Lerp(startColor, targetColor, t));
                yield return null;
            }
            
            // Hold
            yield return new WaitForSeconds(0.5f);
            
            // Fade back
            elapsed = 0;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                postProcessMaterial.SetColor("_ColorTint", Color.Lerp(targetColor, normalColor, t));
                yield return null;
            }
        }
        
        private IEnumerator ChromaticAberrationRoutine(float targetIntensity, float duration)
        {
            float startIntensity = postProcessMaterial.GetFloat("_ChromaticAberration");
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                postProcessMaterial.SetFloat("_ChromaticAberration", intensity);
                yield return null;
            }
        }
        
        public void ActivateDamageEffect()
        {
            StartCoroutine(DamageFlashRoutine());
        }
        
        private IEnumerator DamageFlashRoutine()
        {
            float duration = 0.1f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                postProcessMaterial.SetColor("_ColorTint", Color.Lerp(damageColor, normalColor, t));
                yield return null;
            }
            
            postProcessMaterial.SetColor("_ColorTint", normalColor);
        }
        
        public void ActivateSlowMotion(float slowFactor, float duration)
        {
            StartCoroutine(SlowMotionRoutine(slowFactor, duration));
        }
        
        private IEnumerator SlowMotionRoutine(float slowFactor, float duration)
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = slowFactor;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            
            yield return new WaitForSecondsRealtime(duration);
            
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = 0.02f;
        }
        
        private void OnDestroy()
        {
            // Reset time scale if active
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}
