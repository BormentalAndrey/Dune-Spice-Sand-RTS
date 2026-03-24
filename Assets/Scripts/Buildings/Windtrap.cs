using UnityEngine;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Windtrap - Collects atmospheric moisture
    /// References: Dune, Book I, Chapter 6, "The windtraps of Arrakis"
    /// </summary>
    public class Windtrap : BuildingBase
    {
        [Header("Water Production")]
        public float waterPerSecond = 1f;
        public float efficiency = 1f; // Affected by sandstorms
        
        [Header("Storage")]
        public float waterStorage = 0f;
        public float maxStorage = 200f;
        
        [Header("Sandstorm Effect - Dune, Book I, Chapter 16")]
        public bool isInSandstorm = false;
        public float sandstormEfficiencyPenalty = 0.5f;
        
        private void Update()
        {
            if (!isConstructed) return;
            
            // Calculate current efficiency
            float currentEfficiency = efficiency;
            if (isInSandstorm)
            {
                currentEfficiency *= sandstormEfficiencyPenalty;
            }
            
            // Produce water
            float waterProduced = waterPerSecond * currentEfficiency * Time.deltaTime;
            waterStorage = Mathf.Min(waterStorage + waterProduced, maxStorage);
            
            // Transfer to global water supply (optional - can be collected manually)
            // For now, automatically transfer to main storage
            if (waterStorage > 0)
            {
                float transferAmount = waterStorage * Time.deltaTime;
                waterStorage -= transferAmount;
                GameManager.Instance.AddWater(transferAmount);
            }
        }
        
        public void OnSandstormEnter()
        {
            isInSandstorm = true;
            // TODO: Play sandstorm effect on windtrap
        }
        
        public void OnSandstormExit()
        {
            isInSandstorm = false;
        }
        
        protected override void OnConstructionComplete()
        {
            base.OnConstructionComplete();
            // Initial water storage
            waterStorage = 50f;
        }
    }
}
