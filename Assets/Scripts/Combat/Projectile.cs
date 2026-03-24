using UnityEngine;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Combat
{
    /// <summary>
    /// Projectile system for ranged combat
    /// References: Lasguns, projectile weapons, shield interactions
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Properties")]
        public float speed = 30f;
        public float damage = 25f;
        public UnitBase.AttackType attackType;
        public UnitBase sourceUnit;
        
        [Header("Visual")]
        public TrailRenderer trail;
        public Light projectileLight;
        public GameObject impactEffect;
        
        [Header("Lasgun Properties - Dune, Book I, Chapter 15")]
        public bool isLasgun = false;
        public float lasgunRange = 100f;
        
        private Vector3 direction;
        private float lifeTime = 0f;
        private float maxLifeTime = 10f;
        
        public void Initialize(Vector3 targetPosition, UnitBase source)
        {
            direction = (targetPosition - transform.position).normalized;
            sourceUnit = source;
            
            // Set rotation to face direction
            transform.rotation = Quaternion.LookRotation(direction);
            
            // Lasgun visual effect
            if (isLasgun && projectileLight != null)
            {
                projectileLight.intensity = 2f;
                StartCoroutine(LasgunEffect());
            }
        }
        
        private void Update()
        {
            // Move projectile
            transform.position += direction * speed * Time.deltaTime;
            lifeTime += Time.deltaTime;
            
            if (lifeTime >= maxLifeTime)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check for shield hit first
            ShieldGenerator shield = other.GetComponent<ShieldGenerator>();
            if (shield != null && attackType == UnitBase.AttackType.Projectile)
            {
                // Shield absorbs projectile
                if (shield.AbsorbDamage(damage, attackType))
                {
                    OnImpact(other.transform.position);
                    Destroy(gameObject);
                    return;
                }
            }
            
            // Check for unit hit
            UnitBase unit = other.GetComponent<UnitBase>();
            if (unit != null)
            {
                // Lasgun + Shield = explosion
                if (isLasgun && unit.shieldActive)
                {
                    TriggerLasgunShieldExplosion(unit);
                }
                
                unit.TakeDamage(damage, attackType, sourceUnit);
                OnImpact(unit.transform.position);
                Destroy(gameObject);
            }
            
            // Check for building hit
            BuildingBase building = other.GetComponent<BuildingBase>();
            if (building != null)
            {
                building.TakeDamage(damage);
                OnImpact(building.transform.position);
                Destroy(gameObject);
            }
            
            // Hit terrain
            if (other.CompareTag("Terrain") || other.CompareTag("Sand"))
            {
                OnImpact(transform.position);
                Destroy(gameObject);
            }
        }
        
        private void OnImpact(Vector3 position)
        {
            // Spawn impact effect
            if (impactEffect != null)
            {
                GameObject impact = Instantiate(impactEffect, position, Quaternion.identity);
                Destroy(impact, 1f);
            }
            
            // Play sound
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null)
                audio.Play();
        }
        
        private void TriggerLasgunShieldExplosion(UnitBase target)
        {
            // Nuclear explosion - Dune, Book I, Chapter 15
            float explosionRadius = 15f;
            float explosionDamage = 500f;
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hit in hitColliders)
            {
                UnitBase unit = hit.GetComponent<UnitBase>();
                if (unit != null)
                {
                    unit.TakeDamage(explosionDamage, UnitBase.AttackType.Explosion, sourceUnit);
                }
                
                BuildingBase building = hit.GetComponent<BuildingBase>();
                if (building != null)
                {
                    building.TakeDamage(explosionDamage);
                }
            }
            
            // Visual explosion
            // TODO: Add nuclear explosion VFX
        }
        
        private System.Collections.IEnumerator LasgunEffect()
        {
            // Lasgun beam effect
            while (projectileLight != null && gameObject.activeSelf)
            {
                projectileLight.intensity = 1f + Mathf.Sin(Time.time * 50f) * 0.5f;
                yield return null;
            }
        }
        
        private void OnDestroy()
        {
            if (trail != null)
                trail.transform.SetParent(null);
        }
    }
}
