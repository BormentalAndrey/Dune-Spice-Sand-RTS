using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Visual
{
    /// <summary>
    /// Particle effect system for Dune-specific visuals
    /// Sandstorms, spice blooms, shield effects, worm emergence
    /// </summary>
    public class ParticleEffects : MonoBehaviour
    {
        public static ParticleEffects Instance { get; private set; }
        
        [Header("Effect Prefabs")]
        public GameObject sandExplosionPrefab;
        public GameObject spiceBloomPrefab;
        public GameObject shieldHitPrefab;
        public GameObject wormEmergencePrefab;
        public GameObject lasgunBeamPrefab;
        public GameObject voiceWavePrefab;
        
        [Header("Spice Effects")]
        public ParticleSystem spiceParticles;
        public Light spiceGlow;
        
        [Header("Sand Effects")]
        public ParticleSystem sandstormParticles;
        public ParticleSystem sandTrailParticles;
        
        [Header("Shield Effects")]
        public Material shieldMaterial;
        public float shieldPulseSpeed = 2f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public void SpawnSandExplosion(Vector3 position, float size = 1f)
        {
            if (sandExplosionPrefab != null)
            {
                GameObject explosion = Instantiate(sandExplosionPrefab, position, Quaternion.identity);
                explosion.transform.localScale = Vector3.one * size;
                Destroy(explosion, 2f);
            }
        }
        
        public void SpawnSpiceBloom(Vector3 position)
        {
            if (spiceBloomPrefab != null)
            {
                GameObject bloom = Instantiate(spiceBloomPrefab, position, Quaternion.identity);
                Destroy(bloom, 3f);
                
                // Spice glow effect
                StartCoroutine(SpiceGlowEffect(position));
            }
        }
        
        private IEnumerator SpiceGlowEffect(Vector3 position)
        {
            Light glow = new GameObject("SpiceGlow").AddComponent<Light>();
            glow.transform.position = position;
            glow.color = new Color(1f, 0.5f, 0.2f);
            glow.intensity = 2f;
            glow.range = 10f;
            
            float t = 0;
            while (t < 2f)
            {
                t += Time.deltaTime;
                glow.intensity = Mathf.Lerp(2f, 0, t / 2f);
                yield return null;
            }
            
            Destroy(glow.gameObject);
        }
        
        public void SpawnShieldHit(Vector3 position)
        {
            if (shieldHitPrefab != null)
            {
                GameObject hit = Instantiate(shieldHitPrefab, position, Quaternion.identity);
                Destroy(hit, 0.5f);
            }
        }
        
        public void SpawnWormEmergence(Vector3 position)
        {
            if (wormEmergencePrefab != null)
            {
                GameObject wormEffect = Instantiate(wormEmergencePrefab, position, Quaternion.identity);
                Destroy(wormEffect, 3f);
                
                // Camera shake
                StartCoroutine(CameraShake(0.5f, 0.3f));
            }
        }
        
        public void SpawnLasgunBeam(Vector3 start, Vector3 end)
        {
            if (lasgunBeamPrefab != null)
            {
                GameObject beam = Instantiate(lasgunBeamPrefab, start, Quaternion.identity);
                
                // Orient beam towards end
                Vector3 direction = end - start;
                beam.transform.rotation = Quaternion.LookRotation(direction);
                
                // Scale beam length
                float length = direction.magnitude;
                beam.transform.localScale = new Vector3(1, 1, length);
                
                Destroy(beam, 0.1f);
            }
        }
        
        public void SpawnVoiceWave(Vector3 position, float radius)
        {
            if (voiceWavePrefab != null)
            {
                GameObject wave = Instantiate(voiceWavePrefab, position, Quaternion.identity);
                
                // Animate wave expansion
                StartCoroutine(ExpandWave(wave, radius));
            }
        }
        
        private IEnumerator ExpandWave(GameObject wave, float maxRadius)
        {
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float radius = Mathf.Lerp(0.1f, maxRadius, t / 0.5f);
                wave.transform.localScale = new Vector3(radius, radius, radius);
                yield return null;
            }
            
            Destroy(wave);
        }
        
        private IEnumerator CameraShake(float duration, float magnitude)
        {
            Camera mainCamera = Camera.main;
            Vector3 originalPosition = mainCamera.transform.localPosition;
            
            float elapsed = 0;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                
                mainCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);
                elapsed += Time.deltaTime;
                
                yield return null;
            }
            
            mainCamera.transform.localPosition = originalPosition;
        }
        
        public void CreateSandTrail(Vector3 start, Vector3 end, float width = 1f)
        {
            if (sandTrailParticles != null)
            {
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.position = start;
                sandTrailParticles.Emit(emitParams, 10);
            }
        }
        
        public void UpdateShieldEffect(Renderer shieldRenderer, float shieldStrength)
        {
            if (shieldRenderer != null && shieldMaterial != null)
            {
                float pulse = Mathf.Sin(Time.time * shieldPulseSpeed) * 0.5f + 0.5f;
                float intensity = 0.3f + (shieldStrength * 0.7f);
                
                shieldRenderer.material.SetFloat("_Intensity", intensity * pulse);
                shieldRenderer.material.SetFloat("_Pulse", pulse);
            }
        }
        
        public void PlaySpiceHarvestEffect(Vector3 position)
        {
            if (spiceParticles != null)
            {
                spiceParticles.transform.position = position;
                spiceParticles.Emit(50);
            }
        }
        
        public void PlaySandstormEffect(Vector3 center, float radius)
        {
            if (sandstormParticles != null)
            {
                sandstormParticles.transform.position = center;
                var main = sandstormParticles.main;
                main.startSize = radius / 10f;
                sandstormParticles.Play();
            }
        }
        
        public void StopSandstormEffect()
        {
            if (sandstormParticles != null)
                sandstormParticles.Stop();
        }
    }
}
