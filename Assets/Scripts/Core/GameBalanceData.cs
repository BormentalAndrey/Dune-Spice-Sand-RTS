using UnityEngine;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Game balance configuration for unit stats, costs, and mechanics
    /// References: Playtesting data and Dune lore accuracy
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalanceData", menuName = "Dune/Balance Data")]
    public class GameBalanceData : ScriptableObject
    {
        [Header("Resource Balance")]
        public float spicePerHarvest = 10f;
        public float harvesterCapacity = 200f;
        public float refineryRefineRate = 5f;
        public float windtrapWaterRate = 1f;
        
        [Header("Unit Costs")]
        public UnitCosts atreidesCosts;
        public UnitCosts harkonnenCosts;
        public UnitCosts fremenCosts;
        
        [Header("Building Costs")]
        public BuildingCosts buildingCosts;
        
        [Header("Combat Balance")]
        public float shieldAbsorptionRate = 0.8f;
        public float lasgunExplosionRadius = 15f;
        public float lasgunExplosionDamage = 500f;
        public float meleeShieldPenetration = 1f;
        
        [Header("Worm Balance")]
        public float wormSpawnThreshold = 5f;
        public float wormVibrationDecay = 2f;
        public float wormDamage = 150f;
        public float wormHealth = 1000f;
        
        [Header("Faction Bonuses")]
        public float atreidesPrescienceCost = 50f;
        public float harkonnenPoisonDamage = 10f;
        public float fremenStealthBonus = 0.3f;
        
        [System.Serializable]
        public class UnitCosts
        {
            public float trooperSpice = 80f;
            public float trooperWater = 5f;
            public float commandoSpice = 150f;
            public float commandoWater = 15f;
            public float eliteSpice = 300f;
            public float eliteWater = 30f;
            public float vehicleSpice = 400f;
            public float vehicleWater = 40f;
        }
        
        [System.Serializable]
        public class BuildingCosts
        {
            public float commandCenterSpice = 500f;
            public float commandCenterWater = 100f;
            public float barracksSpice = 200f;
            public float barracksWater = 50f;
            public float refinerySpice = 300f;
            public float refineryWater = 75f;
            public float windtrapSpice = 150f;
            public float windtrapWater = 0f;
            public float shieldSpice = 400f;
            public float shieldWater = 100f;
        }
        
        private static GameBalanceData _instance;
        public static GameBalanceData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameBalanceData>("GameBalanceData");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<GameBalanceData>();
                        _instance.SetDefaultValues();
                    }
                }
                return _instance;
            }
        }
        
        private void SetDefaultValues()
        {
            spicePerHarvest = 10f;
            harvesterCapacity = 200f;
            refineryRefineRate = 5f;
            windtrapWaterRate = 1f;
            
            atreidesCosts = new UnitCosts
            {
                trooperSpice = 80f,
                trooperWater = 5f,
                commandoSpice = 150f,
                commandoWater = 15f,
                eliteSpice = 300f,
                eliteWater = 30f,
                vehicleSpice = 400f,
                vehicleWater = 40f
            };
            
            harkonnenCosts = new UnitCosts
            {
                trooperSpice = 60f,
                trooperWater = 3f,
                commandoSpice = 120f,
                commandoWater = 10f,
                eliteSpice = 250f,
                eliteWater = 25f,
                vehicleSpice = 350f,
                vehicleWater = 35f
            };
            
            fremenCosts = new UnitCosts
            {
                trooperSpice = 100f,
                trooperWater = 0f,
                commandoSpice = 150f,
                commandoWater = 0f,
                eliteSpice = 500f,
                eliteWater = 0f,
                vehicleSpice = 300f,
                vehicleWater = 0f
            };
            
            shieldAbsorptionRate = 0.8f;
            lasgunExplosionRadius = 15f;
            lasgunExplosionDamage = 500f;
            meleeShieldPenetration = 1f;
            
            wormSpawnThreshold = 5f;
            wormVibrationDecay = 2f;
            wormDamage = 150f;
            wormHealth = 1000f;
            
            atreidesPrescienceCost = 50f;
            harkonnenPoisonDamage = 10f;
            fremenStealthBonus = 0.3f;
        }
        
        public float GetUnitSpiceCost(string unitName, GameManager.Faction faction)
        {
            UnitCosts costs = GetFactionCosts(faction);
            
            if (unitName.Contains("Trooper") || unitName.Contains("Warrior"))
                return costs.trooperSpice;
            else if (unitName.Contains("Commando") || unitName.Contains("Fedaykin"))
                return costs.commandoSpice;
            else if (unitName.Contains("Elite") || unitName.Contains("Naib") || unitName.Contains("Swordmaster"))
                return costs.eliteSpice;
            else if (unitName.Contains("Vehicle") || unitName.Contains("Tank") || unitName.Contains("Ornithopter"))
                return costs.vehicleSpice;
                
            return costs.trooperSpice;
        }
        
        public float GetUnitWaterCost(string unitName, GameManager.Faction faction)
        {
            if (faction == GameManager.Faction.Fremen)
                return 0f;
                
            UnitCosts costs = GetFactionCosts(faction);
            
            if (unitName.Contains("Trooper") || unitName.Contains("Warrior"))
                return costs.trooperWater;
            else if (unitName.Contains("Commando") || unitName.Contains("Fedaykin"))
                return costs.commandoWater;
            else if (unitName.Contains("Elite") || unitName.Contains("Naib") || unitName.Contains("Swordmaster"))
                return costs.eliteWater;
            else if (unitName.Contains("Vehicle") || unitName.Contains("Tank") || unitName.Contains("Ornithopter"))
                return costs.vehicleWater;
                
            return costs.trooperWater;
        }
        
        private UnitCosts GetFactionCosts(GameManager.Faction faction)
        {
            switch (faction)
            {
                case GameManager.Faction.Atreides:
                    return atreidesCosts;
                case GameManager.Faction.Harkonnen:
                    return harkonnenCosts;
                case GameManager.Faction.Fremen:
                    return fremenCosts;
                default:
                    return atreidesCosts;
            }
        }
        
        public float GetBuildingCost(string buildingName, string costType)
        {
            switch (buildingName.ToLower())
            {
                case "command center":
                    return costType == "spice" ? buildingCosts.commandCenterSpice : buildingCosts.commandCenterWater;
                case "barracks":
                    return costType == "spice" ? buildingCosts.barracksSpice : buildingCosts.barracksWater;
                case "spice refinery":
                    return costType == "spice" ? buildingCosts.refinerySpice : buildingCosts.refineryWater;
                case "windtrap":
                    return costType == "spice" ? buildingCosts.windtrapSpice : buildingCosts.windtrapWater;
                case "shield generator":
                    return costType == "spice" ? buildingCosts.shieldSpice : buildingCosts.shieldWater;
                default:
                    return 0f;
            }
        }
        
        public void LogBalanceData()
        {
            Debug.Log("=== Game Balance Data ===");
            Debug.Log($"Spice per Harvest: {spicePerHarvest}");
            Debug.Log($"Harvester Capacity: {harvesterCapacity}");
            Debug.Log($"Worm Spawn Threshold: {wormSpawnThreshold}");
            Debug.Log($"Lasgun Explosion Radius: {lasgunExplosionRadius}");
            Debug.Log("=========================");
        }
    }
}
