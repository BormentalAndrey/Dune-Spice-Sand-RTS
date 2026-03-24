using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Buildings;

namespace Dune.SpiceAndSand.Units
{
    /// <summary>
    /// Manages unit spawning from buildings
    /// References: Dune, Book I, Chapter 8 - Atreides training grounds
    /// </summary>
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public BuildingBase parentBuilding;
        public Transform spawnPoint;
        public float spawnRadius = 2f;
        
        [Header("Queue System")]
        public Queue<UnitData> productionQueue = new Queue<UnitData>();
        public bool isProducing = false;
        public float currentProductionProgress = 0f;
        
        [Header("Unit Prefabs")]
        public GameObject atreidesTrooperPrefab;
        public GameObject fremenFedaykinPrefab;
        public GameObject harkonnenTrooperPrefab;
        public GameObject ornithopterPrefab;
        public GameObject sandwormPrefab;
        
        private void Start()
        {
            if (spawnPoint == null)
                spawnPoint = transform;
        }
        
        private void Update()
        {
            if (isProducing && productionQueue.Count > 0)
            {
                currentProductionProgress += Time.deltaTime;
                if (currentProductionProgress >= productionQueue.Peek().productionTime)
                {
                    CompleteProduction();
                }
            }
        }
        
        public void QueueUnit(UnitData unitData)
        {
            // Check resources
            if (!GameManager.Instance.ConsumeSpice(unitData.spiceCost))
                return;
            if (!GameManager.Instance.ConsumeWater(unitData.waterCost))
            {
                // Refund spice if water insufficient
                GameManager.Instance.AddSpice(unitData.spiceCost);
                return;
            }
            
            productionQueue.Enqueue(unitData);
            
            if (!isProducing)
            {
                StartProduction();
            }
            
            // Update UI
            UIManager.Instance?.UpdateProductionQueue(productionQueue.Count);
        }
        
        private void StartProduction()
        {
            isProducing = true;
            currentProductionProgress = 0f;
        }
        
        private void CompleteProduction()
        {
            if (productionQueue.Count == 0)
            {
                isProducing = false;
                return;
            }
            
            UnitData unitData = productionQueue.Dequeue();
            SpawnUnit(unitData);
            
            // Start next production
            if (productionQueue.Count > 0)
            {
                currentProductionProgress = 0f;
            }
            else
            {
                isProducing = false;
            }
        }
        
        private void SpawnUnit(UnitData unitData)
        {
            GameObject unitPrefab = GetPrefabForUnit(unitData);
            if (unitPrefab == null) return;
            
            // Calculate spawn position
            Vector3 spawnPos = GetSpawnPosition();
            
            // Instantiate unit
            GameObject unitObj = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            UnitBase unit = unitObj.GetComponent<UnitBase>();
            
            if (unit != null)
            {
                unit.unitData = unitData;
                unit.faction = GameManager.Instance.playerFaction;
                
                // Initialize unit from data
                var initMethod = unit.GetType().GetMethod("InitializeFromData");
                if (initMethod != null)
                    initMethod.Invoke(unit, null);
            }
            
            // Play spawn effect
            SpawnEffect(spawnPos);
        }
        
        private GameObject GetPrefabForUnit(UnitData unitData)
        {
            string unitName = unitData.unitName.ToLower();
            
            if (unitName.Contains("trooper") && GameManager.Instance.playerFaction == GameManager.Faction.Atreides)
                return atreidesTrooperPrefab;
            else if (unitName.Contains("fedaykin"))
                return fremenFedaykinPrefab;
            else if (unitName.Contains("harkonnen"))
                return harkonnenTrooperPrefab;
            else if (unitName.Contains("ornithopter"))
                return ornithopterPrefab;
                
            return atreidesTrooperPrefab; // Default
        }
        
        private Vector3 GetSpawnPosition()
        {
            Vector3 spawnPos = spawnPoint.position;
            
            // Add random offset within radius
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            spawnPos.x += randomOffset.x;
            spawnPos.z += randomOffset.y;
            
            // Raycast to ground
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                spawnPos.y = hit.point.y;
            }
            
            return spawnPos;
        }
        
        private void SpawnEffect(Vector3 position)
        {
            // TODO: Add spawn VFX (dust cloud, shield shimmer)
            // Particle system for unit arrival
        }
        
        public void CancelProduction()
        {
            if (productionQueue.Count > 0)
            {
                UnitData cancelled = productionQueue.Dequeue();
                
                // Refund resources
                GameManager.Instance.AddSpice(cancelled.spiceCost);
                GameManager.Instance.AddWater(cancelled.waterCost);
                
                // Update UI
                UIManager.Instance?.UpdateProductionQueue(productionQueue.Count);
            }
            
            if (productionQueue.Count == 0)
            {
                isProducing = false;
            }
        }
        
        public int GetQueueCount()
        {
            return productionQueue.Count;
        }
        
        public float GetProductionProgress()
        {
            if (isProducing && productionQueue.Count > 0)
            {
                return currentProductionProgress / productionQueue.Peek().productionTime;
            }
            return 0f;
        }
    }
}
