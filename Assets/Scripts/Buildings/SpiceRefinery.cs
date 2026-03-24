using UnityEngine;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Spice refinery with harvester - Book-accurate worm attraction
    /// References: Dune, Book I, Chapter 3 (harvesters attract worms)
    /// </summary>
    public class SpiceRefinery : BuildingBase
    {
        [Header("Spice Production")]
        public float spiceStorage = 0f;
        public float maxStorage = 500f;
        public SpiceHarvester attachedHarvester;
        
        [Header("Refining Speed")]
        public float refineRate = 10f; // Spice per second
        public float harvestInterval = 5f;
        private float harvestTimer;
        
        [Header("Worm Risk - Dune, Book I, Chapter 3")]
        public float vibrationFromHarvesting = 2f;
        
        private void Start()
        {
            harvestTimer = harvestInterval;
        }
        
        private void Update()
        {
            if (attachedHarvester != null && attachedHarvester.IsHarvesting)
            {
                // Harvester creates rhythmic vibration
                WormManager.Instance?.RegisterVibration(vibrationFromHarvesting, transform.position);
            }
            
            // Refine stored spice
            if (spiceStorage > 0)
            {
                float refined = Mathf.Min(spiceStorage, refineRate * Time.deltaTime);
                spiceStorage -= refined;
                GameManager.Instance.AddSpice(refined);
            }
        }
        
        public void ProcessHarvestedSpice(float amount)
        {
            spiceStorage = Mathf.Min(spiceStorage + amount, maxStorage);
        }
        
        public void DeployHarvester(Vector3 spiceFieldPosition)
        {
            if (attachedHarvester == null || attachedHarvester.IsDestroyed)
            {
                // Spawn new harvester
                // TODO: Instantiate harvester prefab
                attachedHarvester = null; // Placeholder
            }
            
            attachedHarvester?.StartHarvesting(spiceFieldPosition);
        }
        
        public void RecallHarvester()
        {
            attachedHarvester?.ReturnToRefinery();
        }
    }
    
    /// <summary>
    /// Spice harvester unit
    /// </summary>
    public class SpiceHarvester : UnitBase
    {
        public bool IsHarvesting { get; private set; }
        public bool IsDestroyed { get; private set; }
        public float spiceCollected = 0f;
        public float maxSpiceCapacity = 200f;
        
        private SpiceRefinery homeRefinery;
        private Vector3 currentSpiceField;
        private float harvestRate = 20f;
        
        public void StartHarvesting(Vector3 spiceFieldPosition)
        {
            IsHarvesting = true;
            currentSpiceField = spiceFieldPosition;
            MoveTo(spiceFieldPosition);
        }
        
        public void ReturnToRefinery()
        {
            IsHarvesting = false;
            if (homeRefinery != null)
            {
                MoveTo(homeRefinery.transform.position);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (IsHarvesting && other.CompareTag("SpiceField"))
            {
                // Harvest spice
                StartCoroutine(HarvestSpice(other.GetComponent<SpiceField>()));
            }
            
            if (homeRefinery != null && other.gameObject == homeRefinery.gameObject)
            {
                // Unload spice
                homeRefinery.ProcessHarvestedSpice(spiceCollected);
                spiceCollected = 0;
                
                if (!IsHarvesting)
                {
                    // Wait for next deployment
                }
            }
        }
        
        private System.Collections.IEnumerator HarvestSpice(SpiceField field)
        {
            while (IsHarvesting && spiceCollected < maxSpiceCapacity && field.HasSpice)
            {
                float harvested = Mathf.Min(harvestRate, field.ExtractSpice(harvestRate));
                spiceCollected += harvested;
                yield return new WaitForSeconds(1f);
            }
            
            if (spiceCollected >= maxSpiceCapacity || !field.HasSpice)
            {
                ReturnToRefinery();
            }
        }
    }
}
