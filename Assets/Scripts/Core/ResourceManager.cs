using UnityEngine;
using System.Collections;
using Dune.SpiceAndSand.Buildings;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Manages global resources and production
    /// References: Dune economy, spice as strategic resource
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        [Header("Resource Rates")]
        public float globalSpiceRate = 0f;
        public float globalWaterRate = 0f;
        
        [Header("Spice Market - CHOAM")]
        public float spicePrice = 100f; // Spice price per unit
        public float priceVolatility = 0.1f;
        public float lastPriceUpdate;
        
        [Header("Water Debt")]
        public float waterDebt = 0f;
        public float debtInterestRate = 0.05f;
        
        [Header("Stillsuit Recycling")]
        public float globalRecycleRate = 0f;
        public int stillsuitCount = 0;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            StartCoroutine(UpdateSpiceMarket());
            StartCoroutine(UpdateWaterDebt());
        }
        
        private void Update()
        {
            // Apply global resource rates
            if (globalSpiceRate != 0)
            {
                GameManager.Instance.AddSpice(globalSpiceRate * Time.deltaTime);
            }
            
            if (globalWaterRate != 0)
            {
                GameManager.Instance.AddWater(globalWaterRate * Time.deltaTime);
            }
            
            // Stillsuit recycling
            if (stillsuitCount > 0)
            {
                float recycled = globalRecycleRate * stillsuitCount * Time.deltaTime;
                GameManager.Instance.AddWater(recycled);
            }
        }
        
        private IEnumerator UpdateSpiceMarket()
        {
            // CHOAM price fluctuations - Dune, Book I, Chapter 2
            while (true)
            {
                yield return new WaitForSeconds(60f); // Update every minute
                
                // Random price fluctuation
                float change = Random.Range(-priceVolatility, priceVolatility);
                spicePrice += spicePrice * change;
                spicePrice = Mathf.Max(spicePrice, 50f); // Minimum price
                
                lastPriceUpdate = Time.time;
                
                // Notify UI
                UIManager.Instance?.UpdateSpicePrice(spicePrice);
            }
        }
        
        private IEnumerator UpdateWaterDebt()
        {
            // Water debt system - Dune, Book I, Chapter 4
            while (true)
            {
                yield return new WaitForSeconds(30f);
                
                if (waterDebt > 0)
                {
                    float interest = waterDebt * debtInterestRate;
                    waterDebt += interest;
                    
                    // If debt too high, trigger event
                    if (waterDebt > GameManager.Instance.waterCapacity * 2)
                    {
                        TriggerWaterCrisis();
                    }
                }
            }
        }
        
        public void AddWaterDebt(float amount)
        {
            waterDebt += amount;
        }
        
        public bool PayWaterDebt(float amount)
        {
            if (GameManager.Instance.water >= amount)
            {
                GameManager.Instance.ConsumeWater(amount);
                waterDebt = Mathf.Max(waterDebt - amount, 0);
                return true;
            }
            return false;
        }
        
        private void TriggerWaterCrisis()
        {
            // Water crisis event - Fremen may abandon you
            Debug.LogWarning("WATER CRISIS! Fremen loyalty decreasing.");
            // TODO: Reduce faction loyalty, trigger desertion
        }
        
        public void RegisterStillsuit()
        {
            stillsuitCount++;
            UpdateRecycleRate();
        }
        
        public void UnregisterStillsuit()
        {
            stillsuitCount = Mathf.Max(stillsuitCount - 1, 0);
            UpdateRecycleRate();
        }
        
        private void UpdateRecycleRate()
        {
            // Each stillsuit recycles 0.5 water per second - Dune, Book I, Chapter 1
            globalRecycleRate = stillsuitCount * 0.5f;
        }
        
        public float GetSpiceValue(float spiceAmount)
        {
            // Calculate value in Solaris (Imperial currency)
            return spiceAmount * spicePrice;
        }
        
        public bool SellSpice(float spiceAmount)
        {
            // Sell spice to CHOAM
            if (GameManager.Instance.spice >= spiceAmount)
            {
                GameManager.Instance.ConsumeSpice(spiceAmount);
                float value = GetSpiceValue(spiceAmount);
                // Add to currency if implemented
                return true;
            }
            return false;
        }
        
        public void OnBuildingBuilt(BuildingBase building)
        {
            // Update resource rates based on buildings
            if (building is Windtrap)
            {
                globalWaterRate += 1f;
            }
            else if (building is SpiceRefinery)
            {
                globalSpiceRate += 0.5f;
            }
        }
        
        public void OnBuildingDestroyed(BuildingBase building)
        {
            if (building is Windtrap)
            {
                globalWaterRate -= 1f;
            }
            else if (building is SpiceRefinery)
            {
                globalSpiceRate -= 0.5f;
            }
        }
    }
}
