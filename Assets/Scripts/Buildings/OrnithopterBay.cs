using UnityEngine;
using System.Collections;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Ornithopter production facility - Atreides air power
    /// References: Dune, Book I, Chapter 1 - Ornithopters
    /// </summary>
    public class OrnithopterBay : BuildingBase
    {
        [Header("Ornithopter Production")]
        public UnitSpawner spawner;
        public int maxThopters = 5;
        public int currentThopters = 0;
        
        [Header("Types")]
        public UnitData scoutThopter;
        public UnitData fighterThopter;
        public UnitData transportThopter;
        
        [Header("Repair Bay")]
        public float repairRate = 10f;
        public bool isRepairing = false;
        
        private void Start()
        {
            if (spawner == null)
                spawner = GetComponent<UnitSpawner>();
                
            InitializeThopterTypes();
        }
        
        private void InitializeThopterTypes()
        {
            scoutThopter = CreateInstance<UnitData>();
            scoutThopter.unitName = "Scout Ornithopter";
            scoutThopter.maxHealth = 150f;
            scoutThopter.damage = 20f;
            scoutThopter.speed = 20f;
            scoutThopter.movementType = UnitBase.MovementType.Air;
            scoutThopter.spiceCost = 200f;
            scoutThopter.waterCost = 25f;
            scoutThopter.productionTime = 8f;
            scoutThopter.bookReference = "Dune, Book I, Chapter 1";
            
            fighterThopter = CreateInstance<UnitData>();
            fighterThopter.unitName = "Fighter Ornithopter";
            fighterThopter.maxHealth = 300f;
            fighterThopter.damage = 50f;
            fighterThopter.speed = 15f;
            fighterThopter.movementType = UnitBase.MovementType.Air;
            fighterThopter.spiceCost = 400f;
            fighterThopter.waterCost = 50f;
            fighterThopter.productionTime = 12f;
            fighterThopter.bookReference = "Dune, Book I, Chapter 1";
            
            transportThopter = CreateInstance<UnitData>();
            transportThopter.unitName = "Transport Ornithopter";
            transportThopter.maxHealth = 500f;
            transportThopter.damage = 0f;
            transportThopter.speed = 10f;
            transportThopter.movementType = UnitBase.MovementType.Air;
            transportThopter.spiceCost = 300f;
            transportThopter.waterCost = 40f;
            transportThopter.productionTime = 10f;
            transportThopter.bookReference = "Dune, Book I, Chapter 1";
        }
        
        public void ProduceScout()
        {
            if (currentThopters >= maxThopters) return;
            spawner.QueueUnit(scoutThopter);
            currentThopters++;
        }
        
        public void ProduceFighter()
        {
            if (currentThopters >= maxThopters) return;
            spawner.QueueUnit(fighterThopter);
            currentThopters++;
        }
        
        public void ProduceTransport()
        {
            if (currentThopters >= maxThopters) return;
            spawner.QueueUnit(transportThopter);
            currentThopters++;
        }
        
        public void UpgradeBay()
        {
            float upgradeCost = 800f;
            if (GameManager.Instance.ConsumeSpice(upgradeCost))
            {
                maxThopters += 2;
                repairRate += 5f;
                
                // Visual upgrade
                StartCoroutine(UpgradeAnimation());
            }
        }
        
        private IEnumerator UpgradeAnimation()
        {
            // Spawn upgrade particles
            ParticleSystem particles = GetComponent<ParticleSystem>();
            if (particles != null)
                particles.Play();
                
            yield return new WaitForSeconds(1f);
        }
        
        public void RepairThopter(UnitBase thopter)
        {
            StartCoroutine(RepairRoutine(thopter));
        }
        
        private IEnumerator RepairRoutine(UnitBase thopter)
        {
            isRepairing = true;
            
            while (thopter.currentHealth < thopter.maxHealth)
            {
                thopter.currentHealth += repairRate * Time.deltaTime;
                thopter.currentHealth = Mathf.Min(thopter.currentHealth, thopter.maxHealth);
                yield return null;
            }
            
            isRepairing = false;
        }
        
        protected override void OnConstructionComplete()
        {
            base.OnConstructionComplete();
            ResourceManager.Instance?.OnBuildingBuilt(this);
        }
        
        protected override void DestroyBuilding()
        {
            // Destroy all ornithopters when bay is destroyed
            UnitBase[] allUnits = FindObjectsOfType<UnitBase>();
            foreach (var unit in allUnits)
            {
                if (unit.movementType == UnitBase.MovementType.Air && 
                    Vector3.Distance(unit.transform.position, transform.position) < 30f)
                {
                    unit.TakeDamage(9999f, UnitBase.AttackType.Explosion, null);
                }
            }
            
            base.DestroyBuilding();
        }
    }
}
