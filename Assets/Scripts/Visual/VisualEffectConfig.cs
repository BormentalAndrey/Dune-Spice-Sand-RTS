using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Visual
{
    /// <summary>
    /// Visual effect configuration for Dune-specific visuals
    /// References: Sandstorms, shield effects, spice glows
    /// </summary>
    [CreateAssetMenu(fileName = "VisualEffectConfig", menuName = "Dune/Visual Effect Config")]
    public class VisualEffectConfig : ScriptableObject
    {
        [Header("Shield Effects")]
        public Material shieldMaterial;
        public Color shieldActiveColor = new Color(0.5f, 0.8f, 1f, 0.6f);
        public Color shieldLowColor = new Color(1f, 0.3f, 0.3f, 0.6f);
        public float shieldPulseSpeed = 2f;
        public float shieldPulseAmplitude = 0.2f;
        
        [Header("Spice Effects")]
        public Material spiceBloomMaterial;
        public Gradient spiceBloomGradient;
        public float spiceParticleSize = 0.5f;
        public int spiceParticleCount = 50;
        
        [Header("Sand Effects")]
        public Material sandMaterial;
        public Texture2D sandNormalMap;
        public float sandRippleSpeed = 0.5f;
        public float sandRippleScale = 2f;
        
        [Header("Worm Effects")]
        public Material wormMaterial;
        public Color wormGlowColor = new Color(1f, 0.5f, 0.2f);
        public float wormGlowIntensity = 2f;
        
        [Header("Lasgun Effects")]
        public Gradient lasgunBeamGradient;
        public float lasgunBeamWidth = 0.1f;
        public float lasgunBeamDuration = 0.1f;
        
        [Header("Voice Effects")]
        public Material voiceWaveMaterial;
        public Color voiceWaveColor = new Color(1f, 0.2f, 0.8f);
        public float voiceWaveSpeed = 5f;
        
        [Header("Sandstorm Effects")]
        public ParticleSystem.MinMaxGradient sandstormColor;
        public float sandstormParticleSize = 0.2f;
        public int sandstormParticleCount = 1000;
        
        private static VisualEffectConfig _instance;
        public static VisualEffectConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<VisualEffectConfig>("VisualEffectConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("VisualEffectConfig not found in Resources!");
                    }
                }
                return _instance;
            }
        }
        
        public void ApplyShieldMaterial(Renderer renderer, float shieldStrength)
        {
            if (renderer == null || shieldMaterial == null) return;
            
            renderer.material = shieldMaterial;
            
            // Color based on shield strength
            Color shieldColor = Color.Lerp(shieldLowColor, shieldActiveColor, shieldStrength);
            renderer.material.SetColor("_Color", shieldColor);
            
            // Pulse effect
            float pulse = Mathf.Sin(Time.time * shieldPulseSpeed) * shieldPulseAmplitude;
            renderer.material.SetFloat("_Pulse", pulse);
            renderer.material.SetFloat("_Strength", shieldStrength);
        }
        
        public void CreateSpiceBloom(Vector3 position, System.Action onComplete = null)
        {
            if (spiceBloomMaterial == null) return;
            
            // Create bloom effect
            GameObject bloomObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bloomObj.transform.position = position;
            bloomObj.transform.localScale = Vector3.one * 2f;
            
            Renderer renderer = bloomObj.GetComponent<Renderer>();
            renderer.material = spiceBloomMaterial;
            renderer.material.SetColor("_Color", spiceBloomGradient.Evaluate(0));
            
            // Animate bloom
            MonoBehaviour runner = FindObjectOfType<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AnimateSpiceBloom(bloomObj, onComplete));
            }
        }
        
        private IEnumerator AnimateSpiceBloom(GameObject bloomObj, System.Action onComplete)
        {
            float duration = 2f;
            float elapsed = 0;
            
            Renderer renderer = bloomObj.GetComponent<Renderer>();
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Expand and fade
                float scale = 2f + t * 5f;
                bloomObj.transform.localScale = Vector3.one * scale;
                
                Color color = spiceBloomGradient.Evaluate(t);
                renderer.material.SetColor("_Color", color);
                
                yield return null;
            }
            
            Destroy(bloomObj);
            onComplete?.Invoke();
        }
        
        public void CreateSandRipple(Vector3 position, float radius)
        {
            if (sandMaterial == null) return;
            
            GameObject rippleObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rippleObj.transform.position = position + Vector3.up * 0.1f;
            rippleObj.transform.localScale = new Vector3(radius, 0.05f, radius);
            
            Renderer renderer = rippleObj.GetComponent<Renderer>();
            renderer.material = sandMaterial;
            
            MonoBehaviour runner = FindObjectOfType<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AnimateSandRipple(rippleObj));
            }
        }
        
        private IEnumerator AnimateSandRipple(GameObject rippleObj)
        {
            float duration = 1f;
            float elapsed = 0;
            
            Renderer renderer = rippleObj.GetComponent<Renderer>();
            Vector3 startScale = rippleObj.transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Expand outward
                float scale = startScale.x + t * 5f;
                rippleObj.transform.localScale = new Vector3(scale, 0.05f, scale);
                
                // Fade out
                Color color = renderer.material.color;
                color.a = 1 - t;
                renderer.material.color = color;
                
                yield return null;
            }
            
            Destroy(rippleObj);
        }
        
        public void CreateWormGlow(Vector3 position, float intensity)
        {
            if (wormMaterial == null) return;
            
            GameObject glowObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glowObj.transform.position = position;
            glowObj.transform.localScale = Vector3.one * 3f;
            
            Renderer renderer = glowObj.GetComponent<Renderer>();
            renderer.material = wormMaterial;
            renderer.material.SetColor("_Color", wormGlowColor);
            renderer.material.SetFloat("_Intensity", intensity);
            
            MonoBehaviour runner = FindObjectOfType<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AnimateWormGlow(glowObj));
            }
        }
        
        private IEnumerator AnimateWormGlow(GameObject glowObj)
        {
            float duration = 0.5f;
            float elapsed = 0;
            
            Renderer renderer = glowObj.GetComponent<Renderer>();
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float intensity = Mathf.Lerp(2f, 0, t);
                renderer.material.SetFloat("_Intensity", intensity);
                
                yield return null;
            }
            
            Destroy(glowObj);
        }
        
        public void CreateVoiceWave(Vector3 position, float radius)
        {
            if (voiceWaveMaterial == null) return;
            
            GameObject waveObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            waveObj.transform.position = position;
            waveObj.transform.localScale = new Vector3(0.1f, 0.05f, 0.1f);
            
            Renderer renderer = waveObj.GetComponent<Renderer>();
            renderer.material = voiceWaveMaterial;
            renderer.material.SetColor("_Color", voiceWaveColor);
            
            MonoBehaviour runner = FindObjectOfType<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AnimateVoiceWave(waveObj, radius));
            }
        }
        
        private IEnumerator AnimateVoiceWave(GameObject waveObj, float maxRadius)
        {
            float duration = 0.5f;
            float elapsed = 0;
            
            Renderer renderer = waveObj.GetComponent<Renderer>();
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float radius = Mathf.Lerp(0.1f, maxRadius, t);
                waveObj.transform.localScale = new Vector3(radius, 0.05f, radius);
                
                Color color = voiceWaveColor;
                color.a = 1 - t;
                renderer.material.SetColor("_Color", color);
                
                yield return null;
            }
            
            Destroy(waveObj);
        }
        
        public void CreateLasgunBeam(Vector3 start, Vector3 end)
        {
            if (lasgunBeamGradient == null) return;
            
            GameObject beamObj = new GameObject("LasgunBeam");
            LineRenderer line = beamObj.AddComponent<LineRenderer>();
            
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = lasgunBeamWidth;
            line.endWidth = lasgunBeamWidth;
            
            // Apply gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(lasgunBeamGradient.Evaluate(0), 0f),
                    new GradientColorKey(lasgunBeamGradient.Evaluate(0.5f), 0.5f),
                    new GradientColorKey(lasgunBeamGradient.Evaluate(1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            
            line.colorGradient = gradient;
            
            MonoBehaviour runner = FindObjectOfType<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AnimateLasgunBeam(beamObj));
            }
        }
        
        private IEnumerator AnimateLasgunBeam(GameObject beamObj)
        {
            yield return new WaitForSeconds(lasgunBeamDuration);
            Destroy(beamObj);
        }
        
        public void CreateSandstormEffect(Vector3 center, float radius)
        {
            if (sandstormColor == null) return;
            
            GameObject stormObj = new GameObject("Sandstorm");
            ParticleSystem particles = stormObj.AddComponent<ParticleSystem>();
            
            var main = particles.main;
            main.startLifetime = 2f;
            main.startSpeed = 5f;
            main.startSize = sandstormParticleSize;
            main.maxParticles = sandstormParticleCount;
            main.startColor = sandstormColor;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = radius;
            
            var emission = particles.emission;
            emission.rateOverTime = sandstormParticleCount / 2f;
            
            stormObj.transform.position = center;
            
            MonoBehaviour runner = FindObjectOfType<MonoBehaviour>();
            if (runner != null)
            {
                runner.StartCoroutine(AnimateSandstorm(stormObj));
            }
        }
        
        private IEnumerator AnimateSandstorm(GameObject stormObj)
        {
            float duration = 30f;
            float elapsed = 0;
            
            ParticleSystem particles = stormObj.GetComponent<ParticleSystem>();
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // Random movement
                Vector3 offset = Random.insideUnitSphere * 5f;
                offset.y = 0;
                stormObj.transform.position += offset * Time.deltaTime;
                
                yield return null;
            }
            
            var emission = particles.emission;
            emission.rateOverTime = 0;
            
            yield return new WaitForSeconds(2f);
            Destroy(stormObj);
        }
    }
}
