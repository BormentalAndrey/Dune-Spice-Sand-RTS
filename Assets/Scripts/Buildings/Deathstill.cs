using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Buildings
{
    /// <summary>
    /// Fremen deathstill - Extracts water from the dead
    /// References: Dune, Book I, Chapter 1 - Fremen water discipline
    /// </summary>
    public class Deathstill : BuildingBase
    {
        [Header("Water Extraction")]
        public float waterStorage = 0f;
        public float maxStorage = 1000f;
        public float extractionEfficiency = 0.8f; // 80% of body water extracted
        
        [Header("Processing")]
        public float processingRate = 5f;
        public Queue<UnitBase> corpseQueue = new Queue<UnitBase>();
        public bool isProcessing = false;
        
        [Header("Fremen Rituals")]
        public bool isActive = true;
        public float ritualBonus = 1.2f; // During Water of Life ritual
        
        [Header("Visual")]
        public ParticleSystem extractionParticles;
        public Light waterGlow;
        
        private void Update()
        {
            if (!isConstructed) return;
            
            ProcessQueue();
            UpdateVisuals();
        }
        
        public void AddCorpse(UnitBase corpse)
        {
            if (!isActive) return;
            
            corpseQueue.Enqueue(corpse);
            
            // Fremen death ritual - Dune, Book I, Chapter 1
            Debug.Log($"Fremen: 'His water belongs to the tribe.'");
        }
        
        private void ProcessQueue()
        {
            if (isProcessing || corpseQueue.Count == 0) return;
            
            StartCoroutine(ProcessCorpse());
        }
        
        private IEnumerator ProcessCorpse()
        {
            isProcessing = true;
            
            while (corpseQueue.Count > 0 && waterStorage < maxStorage)
            {
                UnitBase corpse = corpseQueue.Dequeue();
                
                // Calculate water from corpse
                float waterAmount = CalculateWaterFromCorpse(corpse);
                waterAmount *= extractionEfficiency;
                
                if (ritualBonus > 1f)
                    waterAmount *= ritualBonus;
                    
                waterStorage = Mathf.Min(waterStorage + waterAmount, maxStorage);
                
                // Transfer to global water
                GameManager.Instance.AddWater(waterAmount);
                
                // Visual effect
                if (extractionParticles != null)
                    extractionParticles.Play();
                    
                // Play Fremen chant
                AudioSource audio = GetComponent<AudioSource>();
                if (audio != null)
                    audio.Play();
                    
                yield return new WaitForSeconds(processingRate);
            }
            
            isProcessing = false;
        }
        
        private float CalculateWaterFromCorpse(UnitBase corpse)
        {
            // Water content based on unit type - Dune, Book I, Chapter 1
            float baseWater = 50f;
            
            if (corpse is FremenUnit)
                baseWater = 80f; // Fremen carry more water
            else if (corpse is HarkonnenUnit)
                baseWater = 60f;
            else if (corpse is SardaukarUnit)
                baseWater = 70f;
                
            // Scale by size
            float sizeScale = corpse.maxHealth / 100f;
            return baseWater * sizeScale;
        }
        
        private void UpdateVisuals()
        {
            if (waterGlow != null)
            {
                // Glow intensity based on water storage
                float intensity = waterStorage / maxStorage;
                waterGlow.intensity = 0.5f + intensity * 2f;
            }
        }
        
        public void ActivateRitual()
        {
            // Water of Life ritual - Dune, Book I, Chapter 36
            if (GameManager.Instance.ConsumeSpice(200f))
            {
                ritualBonus = 1.5f;
                StartCoroutine(RitualDuration());
            }
        }
        
        private IEnumerator RitualDuration()
        {
            // Visual ritual effect
            ParticleSystem ritualParticles = GetComponent<ParticleSystem>();
            if (ritualParticles != null)
                ritualParticles.Play();
                
            yield return new WaitForSeconds(60f);
            
            ritualBonus = 1.2f;
        }
        
        public float CollectWater()
        {
            float collected = waterStorage;
            waterStorage = 0f;
            return collected;
        }
        
        protected override void OnConstructionComplete()
        {
            base.OnConstructionComplete();
            
            // Fremen water discipline
            GameManager.Instance.AddWater(200f);
        }
    }
}
