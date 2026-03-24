using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Ecology
{
    /// <summary>
    /// Sandstorm environmental hazard
    /// References: Dune, Book I, Chapter 16 - Coriolis storms
    /// </summary>
    public class Sandstorm : MonoBehaviour
    {
        [Header("Storm Properties")]
        public float stormRadius = 100f;
        public float stormSpeed = 10f;
        public float damagePerSecond = 5f;
        public float visionReduction = 0.8f;
        public float movementSlow = 0.5f;
        
        [Header("Visual Effects")]
        public ParticleSystem sandParticles;
        public Light stormLight;
        public AudioSource windSound;
        
        [Header("Direction")]
        public Vector3 movementDirection;
        
        private bool isActive = false;
        private float originalLightIntensity;
        
        private void Start()
        {
            if (stormLight != null)
                originalLightIntensity = stormLight.intensity;
                
            StartCoroutine(StormMovement());
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            
            // Damage units in sandstorm
            UnitBase unit = other.GetComponent<UnitBase>();
            if (unit != null)
            {
                StartCoroutine(DamageUnit(unit));
            }
            
            // Slow movement
            if (unit != null && unit.navAgent != null)
            {
                unit.navAgent.speed *= movementSlow;
            }
            
            // Reduce vision for player units
            if (unit != null && unit.faction == GameManager.Instance.playerFaction)
            {
                ApplyVisionDebuff(unit);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            // Restore movement speed
            UnitBase unit = other.GetComponent<UnitBase>();
            if (unit != null && unit.navAgent != null)
            {
                if (unit.unitData != null)
                    unit.navAgent.speed = unit.unitData.speed;
                else
                    unit.navAgent.speed = unit.speed;
            }
            
            // Restore vision
            RestoreVision(unit);
        }
        
        private IEnumerator DamageUnit(UnitBase unit)
        {
            while (isActive && unit != null && Vector3.Distance(transform.position, unit.transform.position) < stormRadius)
            {
                unit.TakeDamage(damagePerSecond, UnitBase.AttackType.Explosion, null);
                yield return new WaitForSeconds(1f);
            }
        }
        
        private void ApplyVisionDebuff(UnitBase unit)
        {
            // TODO: Implement vision reduction via fog of war
            // For now, add visual effect
            var renderer = unit.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 0.5f;
                renderer.material.color = color;
            }
        }
        
        private void RestoreVision(UnitBase unit)
        {
            if (unit != null)
            {
                var renderer = unit.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = 1f;
                    renderer.material.color = color;
                }
            }
        }
        
        private IEnumerator StormMovement()
        {
            isActive = true;
            
            // Play effects
            if (sandParticles != null)
                sandParticles.Play();
            if (windSound != null)
                windSound.Play();
                
            float stormDuration = Random.Range(30f, 90f);
            float timer = 0f;
            
            while (timer < stormDuration)
            {
                timer += Time.deltaTime;
                
                // Move storm
                transform.position += movementDirection * stormSpeed * Time.deltaTime;
                
                // Pulse visual effect
                if (stormLight != null)
                {
                    stormLight.intensity = originalLightIntensity + Mathf.Sin(Time.time * 5f) * 0.5f;
                }
                
                yield return null;
            }
            
            // End storm
            isActive = false;
            if (sandParticles != null)
                sandParticles.Stop();
            if (windSound != null)
                windSound.Stop();
                
            Destroy(gameObject, 2f);
        }
        
        public void Initialize(float radius, Vector3 direction, Vector3 startPosition)
        {
            stormRadius = radius;
            movementDirection = direction.normalized;
            transform.position = startPosition;
            
            // Scale visual effects
            transform.localScale = Vector3.one * radius / 50f;
            
            StartCoroutine(StormMovement());
        }
    }
}
