using UnityEngine;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Ecology;

namespace Dune.SpiceAndSand.Units
{
    /// <summary>
    /// Fremen warrior - Desert power
    /// References: Dune, Book I, Chapter 25-35
    /// </summary>
    public class FremenUnit : UnitBase
    {
        [Header("Fremen Special Abilities")]
        public bool hasStillsuit = true;
        public bool hasCrysknife = true;
        public bool canRideWorm = true;
        
        [Header("Desert Stealth - Dune, Book I, Chapter 28")]
        public bool isStealthed = false;
        public float stealthSpeedBonus = 1.2f;
        public float stealthDetectionRange = 5f;
        
        [Header("Water Discipline")]
        public float waterRecycleRate = 0.1f; // Recycles water per second
        public float waterCarried = 0f;
        
        [Header("Worm Riding")]
        public SandwormAI mountedWorm;
        public bool isRidingWorm = false;
        
        private Material originalMaterial;
        public Material stealthMaterial;
        private Renderer unitRenderer;
        
        protected override void Awake()
        {
            base.Awake();
            unitRenderer = GetComponent<Renderer>();
            if (unitRenderer != null)
                originalMaterial = unitRenderer.material;
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (!isRidingWorm)
            {
                UpdateStealth();
                UpdateWaterRecycling();
            }
        }
        
        private void UpdateStealth()
        {
            // Fremen stealth in desert - Dune, Book I, Chapter 28
            // "The Fremen moved like shadows across the sand"
            
            bool shouldBeStealthed = IsOnSand() && currentState != UnitState.Attacking;
            
            if (shouldBeStealthed && !isStealthed)
            {
                ActivateStealth();
            }
            else if (!shouldBeStealthed && isStealthed)
            {
                DeactivateStealth();
            }
        }
        
        private void ActivateStealth()
        {
            isStealthed = true;
            if (stealthMaterial != null && unitRenderer != null)
                unitRenderer.material = stealthMaterial;
                
            // Reduce vibration signature
            vibrationIntensity *= 0.3f;
            
            // Speed bonus in stealth
            if (navAgent != null)
                navAgent.speed = speed * stealthSpeedBonus;
        }
        
        private void DeactivateStealth()
        {
            isStealthed = false;
            if (originalMaterial != null && unitRenderer != null)
                unitRenderer.material = originalMaterial;
                
            // Restore vibration
            if (unitData != null)
                vibrationIntensity = unitData.vibrationIntensity;
                
            // Restore speed
            if (navAgent != null)
                navAgent.speed = speed;
        }
        
        private void UpdateWaterRecycling()
        {
            // Stillsuit recycles water - Dune, Book I, Chapter 1
            if (hasStillsuit && waterCarried > 0)
            {
                float recycled = waterRecycleRate * Time.deltaTime;
                waterCarried = Mathf.Max(waterCarried - recycled, 0);
                GameManager.Instance.AddWater(recycled);
            }
        }
        
        private bool IsOnSand()
        {
            // Raycast down to check terrain
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
            {
                return hit.collider.CompareTag("Sand");
            }
            return false;
        }
        
        public void RideWorm(SandwormAI worm)
        {
            // Fremen worm riding ritual - Dune, Book I, Chapter 35
            if (!canRideWorm || worm.isRidden) return;
            
            mountedWorm = worm;
            isRidingWorm = true;
            worm.Mount(this);
            
            // Dismount when reaching destination
            StartCoroutine(DismountAfterDelay(30f));
        }
        
        private System.Collections.IEnumerator DismountAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            DismountWorm();
        }
        
        public void DismountWorm()
        {
            if (mountedWorm != null)
            {
                mountedWorm.Dismount();
                mountedWorm = null;
            }
            isRidingWorm = false;
        }
        
        public void UseCrysknife(UnitBase target)
        {
            // Crysknife - Fremen sacred blade - Dune, Book I, Chapter 28
            if (!hasCrysknife) return;
            
            // Crysknife penetrates shields (slow blade)
            float crysknifeDamage = damage * 1.5f;
            target.TakeDamage(crysknifeDamage, AttackType.MeleeBlade, this);
            
            // Dune: Crysknife must not be sheathed without drawing blood
            // Game mechanic: Must kill or will lose honor
            if (target.currentHealth <= 0)
            {
                // Blood drawn - crysknife satisfied
            }
            else
            {
                // No blood - temporary debuff
                damage *= 0.8f;
                StartCoroutine(RestoreCrysknifeHonor());
            }
        }
        
        private System.Collections.IEnumerator RestoreCrysknifeHonor()
        {
            yield return new WaitForSeconds(10f);
            if (unitData != null)
                damage = unitData.damage;
        }
        
        public override void Attack(UnitBase target)
        {
            if (hasCrysknife && Vector3.Distance(transform.position, target.transform.position) < 2f)
            {
                UseCrysknife(target);
            }
            else
            {
                base.Attack(target);
            }
        }
        
        protected override void Die()
        {
            // Fremen water ritual - Dune, Book I, Chapter 1
            // Fremen reclaim water from the dead
            if (waterCarried > 0)
            {
                GameManager.Instance.AddWater(waterCarried);
            }
            
            // Return water to tribe
            GameManager.Instance.AddWater(50f);
            
            base.Die();
        }
        
        public void CollectWaterFromEnemy(UnitBase enemy)
        {
            // Fremen water discipline - Dune, Book I, Chapter 1
            if (enemy is HarkonnenUnit || enemy is SardaukarUnit)
            {
                waterCarried += 100f;
            }
        }
    }
}
