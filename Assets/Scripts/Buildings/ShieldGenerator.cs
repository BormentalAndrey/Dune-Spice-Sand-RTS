using UnityEngine;
using System.Collections;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Holtzman shield generator - Defensive barrier
    /// References: Dune, Book I, Chapter 5 - Personal shields
    /// </summary>
    public class ShieldGenerator : BuildingBase
    {
        [Header("Shield Properties")]
        public float shieldRadius = 30f;
        public float shieldStrength = 1000f;
        public float maxShield = 1000f;
        public float rechargeRate = 20f;
        
        [Header("Shield Effects")]
        public GameObject shieldMesh;
        public Material shieldMaterial;
        public Color shieldColor = Color.cyan;
        
        [Header("Shield Mechanics - Dune, Book I, Chapter 5")]
        public bool blocksProjectiles = true;
        public bool attractsWorms = false; // Shield vibrations attract worms
        public float vibrationFromShield = 0.5f;
        
        [Header("Lasgun Risk")]
        public bool isLasgunHit = false;
        public float explosionRadius = 20f;
        
        private bool isShieldActive = true;
        private float currentShield;
        private float lastDamageTime;
        
        private void Start()
        {
            currentShield = maxShield;
            InitializeShieldVisual();
        }
        
        private void Update()
        {
            if (!isConstructed) return;
            
            UpdateShieldRecharge();
            UpdateShieldVisual();
            EmitVibration();
        }
        
        private void InitializeShieldVisual()
        {
            if (shieldMesh == null)
            {
                // Create shield sphere
                shieldMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shieldMesh.transform.SetParent(transform);
                shieldMesh.transform.localScale = Vector3.one * shieldRadius * 2f;
                shieldMesh.transform.localPosition = Vector3.zero;
                
                // Set material
                Renderer renderer = shieldMesh.GetComponent<Renderer>();
                renderer.material = shieldMaterial ?? new Material(Shader.Find("Transparent/Diffuse"));
                renderer.material.color = shieldColor;
                
                // Make semi-transparent
                Color color = renderer.material.color;
                color.a = 0.3f;
                renderer.material.color = color;
            }
        }
        
        private void UpdateShieldRecharge()
        {
            if (Time.time > lastDamageTime + 2f && currentShield < maxShield)
            {
                currentShield += rechargeRate * Time.deltaTime;
                currentShield = Mathf.Min(currentShield, maxShield);
            }
        }
        
        private void UpdateShieldVisual()
        {
            if (shieldMesh == null) return;
            
            // Pulse when taking damage
            float shieldPercent = currentShield / maxShield;
            float scale = shieldRadius * 2f * (0.9f + Mathf.Sin(Time.time * 10f) * 0.1f * (1f - shieldPercent));
            shieldMesh.transform.localScale = Vector3.one * scale;
            
            // Color based on shield strength
            Renderer renderer = shieldMesh.GetComponent<Renderer>();
            Color color = shieldColor;
            color.a = 0.3f * shieldPercent;
            renderer.material.color = color;
        }
        
        private void EmitVibration()
        {
            if (!attractsWorms) return;
            
            // Shield vibrations attract sandworms - Dune, Book I, Chapter 3
            float vibration = vibrationFromShield * (currentShield / maxShield);
            WormManager.Instance?.RegisterVibration(vibration, transform.position);
        }
        
        public bool AbsorbDamage(float amount, UnitBase.AttackType attackType)
        {
            if (!isShieldActive) return false;
            
            // Lasgun + Shield = nuclear explosion - Dune, Book I, Chapter 15
            if (attackType == UnitBase.AttackType.Lasgun)
            {
                TriggerLasgunExplosion();
                return false;
            }
            
            // Shields block fast projectiles
            if (attackType == UnitBase.AttackType.Projectile && blocksProjectiles)
            {
                currentShield -= amount;
                lastDamageTime = Time.time;
                
                if (currentShield <= 0)
                {
                    CollapseShield();
                }
                
                return true;
            }
            
            // Slow blades penetrate shields
            if (attackType == UnitBase.AttackType.MeleeBlade)
            {
                return false;
            }
            
            return false;
        }
        
        private void CollapseShield()
        {
            isShieldActive = false;
            
            if (shieldMesh != null)
                shieldMesh.SetActive(false);
                
            // Shield collapse effect
            StartCoroutine(ShieldReboot());
        }
        
        private IEnumerator ShieldReboot()
        {
            yield return new WaitForSeconds(10f);
            
            currentShield = maxShield * 0.5f;
            isShieldActive = true;
            
            if (shieldMesh != null)
                shieldMesh.SetActive(true);
        }
        
        private void TriggerLasgunExplosion()
        {
            // Nuclear explosion - Dune, Book I, Chapter 15
            Debug.LogWarning("LASGEN-SHIELD EXPLOSION!");
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var collider in hitColliders)
            {
                UnitBase unit = collider.GetComponent<UnitBase>();
                if (unit != null)
                {
                    unit.TakeDamage(500f, UnitBase.AttackType.Explosion, null);
                }
                
                BuildingBase building = collider.GetComponent<BuildingBase>();
                if (building != null)
                {
                    building.TakeDamage(1000f);
                }
            }
            
            // Destroy shield generator
            TakeDamage(9999f);
            
            // Visual explosion
            // TODO: Add explosion VFX
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Absorb projectiles
            Projectile projectile = other.GetComponent<Projectile>();
            if (projectile != null && blocksProjectiles)
            {
                AbsorbDamage(projectile.damage, projectile.attackType);
                Destroy(projectile.gameObject);
            }
        }
        
        public void UpgradeShield()
        {
            float upgradeCost = 1000f;
            if (GameManager.Instance.ConsumeSpice(upgradeCost))
            {
                maxShield += 500f;
                rechargeRate += 10f;
                shieldRadius += 5f;
                
                // Update visual
                StartCoroutine(UpgradeFlash());
            }
        }
        
        private IEnumerator UpgradeFlash()
        {
            Renderer renderer = shieldMesh.GetComponent<Renderer>();
            Color originalColor = renderer.material.color;
            
            for (int i = 0; i < 3; i++)
            {
                renderer.material.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                renderer.material.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        public bool IsPointProtected(Vector3 point)
        {
            return Vector3.Distance(transform.position, point) <= shieldRadius && isShieldActive;
        }
    }
}
