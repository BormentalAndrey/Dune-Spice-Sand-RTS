using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;
using Dune.SpiceAndSand.Buildings;

namespace Dune.SpiceAndSand.AI
{
    /// <summary>
    /// AI controller for enemy factions
    /// Book-accurate behavior: Harkonnen brutality, Sardaukar elite tactics
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        public GameManager.Faction aiFaction;
        public float difficulty = 1f; // 0.5 - 2.0
        public float decisionInterval = 2f;
        
        [Header("Resource Management")]
        public float spice = 1000f;
        public float water = 500f;
        public List<BuildingBase> buildings = new List<BuildingBase>();
        public List<UnitBase> units = new List<UnitBase>();
        
        [Header("Strategy")]
        public AIStrategy currentStrategy = AIStrategy.Defensive;
        public Vector3 basePosition;
        public float attackThreshold = 0.7f;
        
        [Header("Sardaukar Elite - Dune, Book I, Chapter 15")]
        public bool isSardaukar = false;
        public float eliteBonus = 1.5f;
        
        public enum AIStrategy
        {
            Defensive,
            Aggressive,
            Economic,
            Guerilla // Fremen strategy
        }
        
        private float lastDecisionTime;
        
        private void Start()
        {
            basePosition = transform.position;
            StartCoroutine(ProductionRoutine());
            StartCoroutine(AttackRoutine());
        }
        
        private void Update()
        {
            if (Time.time > lastDecisionTime + decisionInterval)
            {
                MakeDecision();
                lastDecisionTime = Time.time;
            }
        }
        
        private void MakeDecision()
        {
            // Assess situation
            float militaryStrength = CalculateMilitaryStrength();
            float playerStrength = GetPlayerStrength();
            float strengthRatio = militaryStrength / Mathf.Max(playerStrength, 1f);
            
            // Update strategy based on strength ratio
            if (strengthRatio > attackThreshold)
            {
                currentStrategy = AIStrategy.Aggressive;
            }
            else if (strengthRatio < 0.3f)
            {
                currentStrategy = AIStrategy.Defensive;
            }
            
            // Execute strategy
            switch (currentStrategy)
            {
                case AIStrategy.Defensive:
                    DefensiveBehavior();
                    break;
                case AIStrategy.Aggressive:
                    AggressiveBehavior();
                    break;
                case AIStrategy.Economic:
                    EconomicBehavior();
                    break;
                case AIStrategy.Guerilla:
                    GuerillaBehavior();
                    break;
            }
        }
        
        private float CalculateMilitaryStrength()
        {
            float strength = 0f;
            foreach (var unit in units)
            {
                if (unit != null && unit.currentState != UnitBase.UnitState.Dead)
                {
                    strength += unit.damage * (unit.currentHealth / unit.maxHealth);
                }
            }
            return strength;
        }
        
        private float GetPlayerStrength()
        {
            // Find all player units
            UnitBase[] playerUnits = FindObjectsOfType<UnitBase>();
            float strength = 0f;
            foreach (var unit in playerUnits)
            {
                if (unit.faction == GameManager.Instance.playerFaction)
                {
                    strength += unit.damage * (unit.currentHealth / unit.maxHealth);
                }
            }
            return strength;
        }
        
        private void DefensiveBehavior()
        {
            // Build defenses, recall units
            BuildDefensiveStructures();
            
            // Retreat injured units
            foreach (var unit in units)
            {
                if (unit != null && unit.currentHealth < unit.maxHealth * 0.3f)
                {
                    unit.MoveTo(basePosition);
                }
            }
        }
        
        private void AggressiveBehavior()
        {
            // Gather attack force
            List<UnitBase> attackForce = new List<UnitBase>();
            foreach (var unit in units)
            {
                if (unit != null && unit.currentHealth > unit.maxHealth * 0.7f)
                {
                    attackForce.Add(unit);
                }
            }
            
            if (attackForce.Count > 5)
            {
                // Find player base
                BuildingBase playerBase = FindPlayerBase();
                if (playerBase != null)
                {
                    foreach (var unit in attackForce)
                    {
                        unit.MoveTo(playerBase.transform.position);
                    }
                }
            }
        }
        
        private void EconomicBehavior()
        {
            // Focus on spice harvesting
            if (spice < 2000f)
            {
                BuildHarvesters();
            }
            
            // Expand to new spice fields
            ExpandToSpiceFields();
        }
        
        private void GuerillaBehavior()
        {
            // Fremen tactics - hit and run, stealth attacks
            if (aiFaction == GameManager.Faction.Fremen)
            {
                // Use stealth to ambush
                foreach (var unit in units)
                {
                    FremenUnit fremen = unit as FremenUnit;
                    if (fremen != null && !fremen.isStealthed)
                    {
                        // Ambush vulnerable targets
                        UnitBase vulnerable = FindVulnerableTarget();
                        if (vulnerable != null)
                        {
                            fremen.SetTarget(vulnerable);
                        }
                    }
                }
            }
        }
        
        private void BuildDefensiveStructures()
        {
            // Build shield generators, turrets
            // TODO: Implement defensive building logic
        }
        
        private void BuildHarvesters()
        {
            // Build spice harvesters
            // TODO: Implement harvester production
        }
        
        private void ExpandToSpiceFields()
        {
            // Find and claim new spice fields
            // TODO: Implement expansion logic
        }
        
        private BuildingBase FindPlayerBase()
        {
            BuildingBase[] buildings = FindObjectsOfType<BuildingBase>();
            foreach (var building in buildings)
            {
                if (building.faction == GameManager.Instance.playerFaction)
                {
                    return building;
                }
            }
            return null;
        }
        
        private UnitBase FindVulnerableTarget()
        {
            UnitBase[] playerUnits = FindObjectsOfType<UnitBase>();
            UnitBase weakest = null;
            float lowestHealth = float.MaxValue;
            
            foreach (var unit in playerUnits)
            {
                if (unit.faction == GameManager.Instance.playerFaction &&
                    unit.currentHealth < lowestHealth)
                {
                    weakest = unit;
                    lowestHealth = unit.currentHealth;
                }
            }
            
            return weakest;
        }
        
        private IEnumerator ProductionRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);
                
                // Produce units based on resources
                if (spice > 500f && water > 100f)
                {
                    ProduceUnit();
                }
            }
        }
        
        private void ProduceUnit()
        {
            // Find barracks
            BuildingBase barracks = buildings.Find(b => b is Barracks);
            if (barracks != null)
            {
                UnitSpawner spawner = barracks.GetComponent<UnitSpawner>();
                if (spawner != null)
                {
                    UnitData unitToSpawn = GetUnitForFaction();
                    if (unitToSpawn != null)
                    {
                        spawner.QueueUnit(unitToSpawn);
                        spice -= unitToSpawn.spiceCost;
                        water -= unitToSpawn.waterCost;
                    }
                }
            }
        }
        
        private UnitData GetUnitForFaction()
        {
            switch (aiFaction)
            {
                case GameManager.Faction.Atreides:
                    return UnitData.AtreidesTrooper();
                case GameManager.Faction.Harkonnen:
                    return UnitData.HarkonnenTrooper();
                case GameManager.Faction.Fremen:
                    return UnitData.FremenFedaykin();
                default:
                    return UnitData.AtreidesTrooper();
            }
        }
        
        private IEnumerator AttackRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(30f);
                
                if (currentStrategy == AIStrategy.Aggressive)
                {
                    LaunchAttack();
                }
            }
        }
        
        private void LaunchAttack()
        {
            // Gather all combat units
            List<UnitBase> attackers = new List<UnitBase>();
            foreach (var unit in units)
            {
                if (unit != null && unit.damage > 0)
                {
                    attackers.Add(unit);
                }
            }
            
            if (attackers.Count > 0)
            {
                BuildingBase target = FindPlayerBase();
                if (target != null)
                {
                    foreach (var attacker in attackers)
                    {
                        attacker.MoveTo(target.transform.position);
                    }
                    
                    // Sardaukar speech - Dune, Book I, Chapter 15
                    if (isSardaukar)
                    {
                        Debug.Log("Sardaukar: 'Victory or death!'");
                    }
                }
            }
        }
        
        public void RegisterUnit(UnitBase unit)
        {
            if (!units.Contains(unit))
            {
                units.Add(unit);
            }
        }
        
        public void RegisterBuilding(BuildingBase building)
        {
            if (!buildings.Contains(building))
            {
                buildings.Add(building);
            }
        }
        
        public void RemoveUnit(UnitBase unit)
        {
            units.Remove(unit);
        }
        
        public void RemoveBuilding(BuildingBase building)
        {
            buildings.Remove(building);
        }
    }
}
