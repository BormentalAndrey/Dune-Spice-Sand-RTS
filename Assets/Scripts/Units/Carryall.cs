using UnityEngine;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Buildings;

namespace Dune.SpiceAndSand.Units
{
    /// <summary>
    /// Carryall - Transports spice harvesters
    /// References: Dune, Book I, Chapter 3 - Harvester carryalls
    /// </summary>
    public class Carryall : UnitBase
    {
        [Header("Carryall Settings")]
        public float carryCapacity = 1f;
        public float pickupRange = 5f;
        public float transportSpeed = 15f;
        
        [Header("Current Cargo")]
        public SpiceHarvester carriedHarvester;
        public bool isCarrying = false;
        
        [Header("Worm Evacuation - Dune, Book I, Chapter 3")]
        public float evacuationTime = 3f;
        public bool isEvacuating = false;
        
        private Vector3 targetDropPoint;
        
        protected override void Update()
        {
            base.Update();
            
            if (isCarrying && carriedHarvester != null)
            {
                // Carry harvester to safety
                carriedHarvester.transform.position = transform.position;
            }
        }
        
        public void PickupHarvester(SpiceHarvester harvester)
        {
            if (isCarrying || harvester == null) return;
            
            carriedHarvester = harvester;
            isCarrying = true;
            
            // Disable harvester movement
            harvester.enabled = false;
            harvester.transform.SetParent(transform);
            
            // Move to refinery
            StartCoroutine(TransportToRefinery());
        }
        
        private System.Collections.IEnumerator TransportToRefinery()
        {
            // Find nearest refinery
            SpiceRefinery refinery = FindObjectOfType<SpiceRefinery>();
            if (refinery != null)
            {
                targetDropPoint = refinery.transform.position;
                
                while (Vector3.Distance(transform.position, targetDropPoint) > pickupRange)
                {
                    MoveTo(targetDropPoint);
                    yield return null;
                }
                
                DropoffHarvester(refinery);
            }
        }
        
        public void DropoffHarvester(SpiceRefinery refinery)
        {
            if (!isCarrying || carriedHarvester == null) return;
            
            carriedHarvester.transform.SetParent(null);
            carriedHarvester.enabled = true;
            carriedHarvester.transform.position = refinery.transform.position + Vector3.right * 3f;
            
            // Unload spice
            if (carriedHarvester.spiceCollected > 0)
            {
                refinery.ProcessHarvestedSpice(carriedHarvester.spiceCollected);
                carriedHarvester.spiceCollected = 0;
            }
            
            carriedHarvester = null;
            isCarrying = false;
        }
        
        public void EmergencyEvacuation(SpiceHarvester harvester)
        {
            // Emergency worm evacuation - Dune, Book I, Chapter 3
            StartCoroutine(EvacuationRoutine(harvester));
        }
        
        private System.Collections.IEnumerator EvacuationRoutine(SpiceHarvester harvester)
        {
            isEvacuating = true;
            
            // Fly to harvester
            while (Vector3.Distance(transform.position, harvester.transform.position) > pickupRange)
            {
                MoveTo(harvester.transform.position);
                yield return null;
            }
            
            // Pick up
            PickupHarvester(harvester);
            
            // Fly to safe location
            Vector3 safePoint = FindSafeLocation();
            while (Vector3.Distance(transform.position, safePoint) > pickupRange)
            {
                MoveTo(safePoint);
                yield return null;
            }
            
            isEvacuating = false;
        }
        
        private Vector3 FindSafeLocation()
        {
            // Find nearest rock formation (safe from worms)
            // TODO: Implement safe location detection
            return transform.position + Vector3.right * 50f;
        }
        
        public override void TakeDamage(float amount, AttackType sourceType, UnitBase source)
        {
            base.TakeDamage(amount, sourceType, source);
            
            if (currentHealth <= 0 && isCarrying)
            {
                // Destroy carried harvester
                if (carriedHarvester != null)
                    carriedHarvester.TakeDamage(9999f, AttackType.Explosion, null);
            }
        }
    }
}
