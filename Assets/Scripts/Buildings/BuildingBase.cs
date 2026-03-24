using UnityEngine;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Base class for all buildings - Dune architecture
    /// References: Sietches, Shield generators, Windtraps from Dune novels
    /// </summary>
    public abstract class BuildingBase : MonoBehaviour
    {
        [Header("Building Identity")]
        public BuildingData buildingData;
        public string buildingName;
        public Faction faction;
        
        [Header("Construction")]
        public float constructionProgress = 0f;
        public bool isConstructed = false;
        public float constructionTime = 5f;
        
        [Header("Health")]
        public float currentHealth;
        public float maxHealth;
        public float armor;
        
        [Header("Shield - Dune, Book I, Chapter 5")]
        public bool hasShield = false;
        public float shieldStrength;
        public float maxShield;
        public float shieldRechargeRate = 1f;
        private float lastDamageTime;
        
        [Header("Production")]
        public bool isProducing = false;
        public float productionProgress = 0f;
        public float productionTime = 3f;
        
        [Header("Resource Costs")]
        public float spiceCost;
        public float waterCost;
        
        [Header("Visual")]
        public GameObject constructionModel;
        public GameObject completeModel;
        public ParticleSystem constructionEffect;
        
        [Header("Book Reference")]
        public string bookReference;
        
        protected virtual void Start()
        {
            if (buildingData != null)
            {
                InitializeFromData();
            }
            
            StartConstruction();
        }
        
        protected virtual void InitializeFromData()
        {
            buildingName = buildingData.buildingName;
            maxHealth = buildingData.maxHealth;
            currentHealth = maxHealth;
            armor = buildingData.armor;
            maxShield = buildingData.maxShield;
            shieldStrength = maxShield;
            hasShield = buildingData.hasShield;
            spiceCost = buildingData.spiceCost;
            waterCost = buildingData.waterCost;
            constructionTime = buildingData.constructionTime;
            bookReference = buildingData.bookReference;
        }
        
        protected virtual void StartConstruction()
        {
            isConstructed = false;
            constructionProgress = 0f;
            
            if (constructionModel != null)
                constructionModel.SetActive(true);
            if (completeModel != null)
                completeModel.SetActive(false);
                
            StartCoroutine(ConstructionRoutine());
        }
        
        private System.Collections.IEnumerator ConstructionRoutine()
        {
            while (constructionProgress < constructionTime)
            {
                constructionProgress += Time.deltaTime;
                
                // Update construction visual
                if (constructionEffect != null && !constructionEffect.isPlaying)
                    constructionEffect.Play();
                    
                yield return null;
            }
            
            CompleteConstruction();
        }
        
        protected virtual void CompleteConstruction()
        {
            isConstructed = true;
            
            if (constructionModel != null)
                constructionModel.SetActive(false);
            if (completeModel != null)
                completeModel.SetActive(true);
                
            if (constructionEffect != null)
                constructionEffect.Stop();
                
            OnConstructionComplete();
        }
        
        protected virtual void OnConstructionComplete()
        {
            // Override for specific building behavior
        }
        
        protected virtual void Update()
        {
            if (!isConstructed) return;
            
            UpdateShield();
            UpdateProduction();
        }
        
        private void UpdateShield()
        {
            if (!hasShield) return;
            
            if (Time.time > lastDamageTime + 2f)
            {
                shieldStrength = Mathf.Min(shieldStrength + shieldRechargeRate * Time.deltaTime, maxShield);
            }
        }
        
        private void UpdateProduction()
        {
            if (!isProducing) return;
            
            productionProgress += Time.deltaTime;
            if (productionProgress >= productionTime)
            {
                CompleteProduction();
            }
        }
        
        public virtual void TakeDamage(float amount)
        {
            // Shield mechanics for buildings
            if (hasShield && shieldStrength > 0)
            {
                float shieldDamage = Mathf.Min(amount, shieldStrength);
                shieldStrength -= shieldDamage;
                amount -= shieldDamage;
                
                if (shieldStrength <= 0)
                {
                    hasShield = false;
                    OnShieldCollapse();
                }
            }
            
            if (amount > 0)
            {
                float mitigatedDamage = amount - armor;
                mitigatedDamage = Mathf.Max(mitigatedDamage, 1f);
                currentHealth -= mitigatedDamage;
                lastDamageTime = Time.time;
                
                if (currentHealth <= 0)
                {
                    DestroyBuilding();
                }
            }
        }
        
        protected virtual void OnShieldCollapse()
        {
            // Visual effect for shield collapse
            // TODO: Add shield collapse VFX
        }
        
        protected virtual void DestroyBuilding()
        {
            // Building destroyed - Dune: Sardaukar destruction
            // TODO: Add destruction VFX
            Destroy(gameObject);
        }
        
        public virtual void StartProduction()
        {
            if (!isConstructed) return;
            isProducing = true;
            productionProgress = 0f;
        }
        
        protected virtual void CompleteProduction()
        {
            isProducing = false;
            // Override for specific production
        }
        
        public virtual bool CanAfford()
        {
            return GameManager.Instance.ConsumeSpice(spiceCost) && 
                   GameManager.Instance.ConsumeWater(waterCost);
        }
        
        public void OnSelected()
        {
            // Highlight building
            // Show production options
        }
        
        public enum Faction
        {
            Atreides,
            Harkonnen,
            Fremen,
            Neutral
        }
    }
    
    /// <summary>
    /// Building ScriptableObject Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Building", menuName = "Dune/Building Data")]
    public class BuildingData : ScriptableObject
    {
        public string buildingName;
        public float maxHealth = 500f;
        public float armor = 20f;
        public float maxShield = 0f;
        public bool hasShield = false;
        public float spiceCost = 200f;
        public float waterCost = 50f;
        public float constructionTime = 5f;
        public string bookReference;
        
        // Specific building types
        public static BuildingData CommandCenter()
        {
            BuildingData data = CreateInstance<BuildingData>();
            data.buildingName = "Command Center";
            data.maxHealth = 1000f;
            data.armor = 30f;
            data.hasShield = true;
            data.maxShield = 300f;
            data.spiceCost = 500f;
            data.waterCost = 100f;
            data.constructionTime = 10f;
            data.bookReference = "Dune, Book I, Chapter 4";
            return data;
        }
        
        public static BuildingData Windtrap()
        {
            BuildingData data = CreateInstance<BuildingData>();
            data.buildingName = "Windtrap";
            data.maxHealth = 300f;
            data.armor = 10f;
            data.spiceCost = 150f;
            data.waterCost = 0f;
            data.constructionTime = 4f;
            data.bookReference = "Dune, Book I, Chapter 6";
            return data;
        }
        
        public static BuildingData SietchEntrance()
        {
            BuildingData data = CreateInstance<BuildingData>();
            data.buildingName = "Sietch Entrance";
            data.maxHealth = 800f;
            data.armor = 50f;
            data.spiceCost = 0f;
            data.waterCost = 200f;
            data.constructionTime = 8f;
            data.bookReference = "Dune, Book I, Chapter 25";
            return data;
        }
    }
}
