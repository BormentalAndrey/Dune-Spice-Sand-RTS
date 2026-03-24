using UnityEngine;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Military training facility - Produces infantry units
    /// References: Dune, Book I, Chapter 8 - Atreides training grounds
    /// </summary>
    public class Barracks : BuildingBase
    {
        [Header("Training")]
        public List<UnitData> availableUnits = new List<UnitData>();
        public UnitSpawner spawner;
        
        [Header("Training Upgrades")]
        public int trainingLevel = 1;
        public float trainingSpeedBonus = 1f;
        
        [Header("Faction-Specific Training")]
        public bool canTrainFedaykin = false; // Fremen only
        public bool canTrainSwordmaster = false; // Atreides only
        public bool canTrainSlaveSoldier = false; // Harkonnen only
        
        private void Start()
        {
            if (spawner == null)
                spawner = GetComponent<UnitSpawner>();
                
            InitializeAvailableUnits();
        }
        
        private void InitializeAvailableUnits()
        {
            switch (faction)
            {
                case Faction.Atreides:
                    availableUnits.Add(UnitData.AtreidesTrooper());
                    availableUnits.Add(UnitData.AtreidesCommando());
                    if (trainingLevel >= 2)
                        availableUnits.Add(UnitData.Swordmaster());
                    break;
                    
                case Faction.Harkonnen:
                    availableUnits.Add(UnitData.HarkonnenSlave());
                    availableUnits.Add(UnitData.HarkonnenHeavy());
                    if (trainingLevel >= 2)
                        availableUnits.Add(UnitData.HarkonnenElite());
                    break;
                    
                case Faction.Fremen:
                    availableUnits.Add(UnitData.FremenWarrior());
                    availableUnits.Add(UnitData.FremenFedaykin());
                    if (trainingLevel >= 2)
                        availableUnits.Add(UnitData.FremenNaib());
                    break;
            }
        }
        
        public void TrainUnit(int unitIndex)
        {
            if (unitIndex < 0 || unitIndex >= availableUnits.Count) return;
            
            UnitData unitToTrain = availableUnits[unitIndex];
            
            // Apply training speed bonus
            unitToTrain.productionTime /= trainingSpeedBonus;
            
            spawner.QueueUnit(unitToTrain);
        }
        
        public void UpgradeTraining()
        {
            if (trainingLevel >= 3) return;
            
            float upgradeCost = 500f * trainingLevel;
            if (GameManager.Instance.ConsumeSpice(upgradeCost) && 
                GameManager.Instance.ConsumeWater(100f * trainingLevel))
            {
                trainingLevel++;
                trainingSpeedBonus = 1f + (trainingLevel - 1) * 0.25f;
                
                // Add new units
                InitializeAvailableUnits();
                
                // Play upgrade effect
                StartCoroutine(UpgradeEffect());
            }
        }
        
        private System.Collections.IEnumerator UpgradeEffect()
        {
            // Visual upgrade effect
            Vector3 originalScale = transform.localScale;
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                transform.localScale = originalScale * (1f + Mathf.Sin(t * Mathf.PI) * 0.1f);
                yield return null;
            }
            transform.localScale = originalScale;
        }
        
        protected override void OnConstructionComplete()
        {
            base.OnConstructionComplete();
            
            // Notify GameManager
            ResourceManager.Instance?.OnBuildingBuilt(this);
        }
    }
    
    // Additional unit data definitions
    public static class UnitDataExtensions
    {
        public static UnitData AtreidesCommando()
        {
            UnitData data = UnitData.AtreidesTrooper();
            data.unitName = "Atreides Commando";
            data.maxHealth = 180f;
            data.damage = 35f;
            data.speed = 6f;
            data.spiceCost = 150f;
            data.waterCost = 15f;
            data.bookReference = "Dune, Book I, Chapter 8";
            return data;
        }
        
        public static UnitData Swordmaster()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Swordmaster";
            data.maxHealth = 250f;
            data.armor = 25f;
            data.damage = 60f;
            data.attackType = UnitBase.AttackType.MeleeBlade;
            data.speed = 5f;
            data.spiceCost = 300f;
            data.waterCost = 30f;
            data.bookReference = "Dune, Book I, Chapter 5 - Ginaz Swordmasters";
            return data;
        }
        
        public static UnitData HarkonnenSlave()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Harkonnen Slave Soldier";
            data.maxHealth = 80f;
            data.armor = 5f;
            data.damage = 15f;
            data.speed = 4f;
            data.spiceCost = 30f;
            data.waterCost = 0f;
            data.bookReference = "Dune, Book I, Chapter 6";
            return data;
        }
        
        public static UnitData HarkonnenHeavy()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Harkonnen Heavy Infantry";
            data.maxHealth = 200f;
            data.armor = 30f;
            data.damage = 40f;
            data.speed = 3.5f;
            data.spiceCost = 200f;
            data.waterCost = 20f;
            data.bookReference = "Dune, Book I, Chapter 10";
            return data;
        }
        
        public static UnitData FremenWarrior()
        {
            UnitData data = UnitData.FremenFedaykin();
            data.unitName = "Fremen Warrior";
            data.maxHealth = 120f;
            data.damage = 30f;
            data.spiceCost = 100f;
            data.waterCost = 0f;
            data.bookReference = "Dune, Book I, Chapter 25";
            return data;
        }
        
        public static UnitData FremenNaib()
        {
            UnitData data = UnitData.FremenFedaykin();
            data.unitName = "Fremen Naib";
            data.maxHealth = 300f;
            data.damage = 80f;
            data.speed = 7f;
            data.spiceCost = 500f;
            data.bookReference = "Dune, Book I, Chapter 25 - Stilgar";
            return data;
        }
        
        public static UnitData HarkonnenElite()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Harkonnen Elite Guard";
            data.maxHealth = 250f;
            data.armor = 35f;
            data.damage = 55f;
            data.speed = 5f;
            data.spiceCost = 400f;
            data.waterCost = 40f;
            data.bookReference = "Dune, Book I, Chapter 15";
            return data;
        }
    }
}
