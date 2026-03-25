using UnityEngine;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Assets
{
    /// <summary>
    /// Central prefab reference manager for efficient asset loading
    /// </summary>
    [CreateAssetMenu(fileName = "PrefabReferences", menuName = "Dune/Prefab References")]
    public class PrefabReferences : ScriptableObject
    {
        [Header("Unit Prefabs")]
        public GameObject atreidesTrooper;
        public GameObject atreidesCommando;
        public GameObject swordmaster;
        public GameObject ornithopterScout;
        public GameObject ornithopterFighter;
        public GameObject carryall;
        
        public GameObject harkonnenSlave;
        public GameObject harkonnenHeavy;
        public GameObject harkonnenElite;
        public GameObject harkonnenTank;
        
        public GameObject fremenWarrior;
        public GameObject fremenFedaykin;
        public GameObject fremenNaib;
        public GameObject sandworm;
        
        [Header("Building Prefabs")]
        public GameObject commandCenter;
        public GameObject barracks;
        public GameObject spiceRefinery;
        public GameObject windtrap;
        public GameObject ornithopterBay;
        public GameObject shieldGenerator;
        public GameObject deathstill;
        public GameObject sietchEntrance;
        
        [Header("Effect Prefabs")]
        public GameObject sandExplosion;
        public GameObject spiceBloom;
        public GameObject shieldHit;
        public GameObject wormEmergence;
        public GameObject lasgunBeam;
        public GameObject voiceWave;
        
        [Header("UI Prefabs")]
        public GameObject unitSelectionPanel;
        public GameObject buildingMenu;
        public GameObject resourcePanel;
        public GameObject minimap;
        
        [Header("Audio Clips")]
        public AudioClip desertAmbience;
        public AudioClip combatMusic;
        public AudioClip fremenChant;
        public AudioClip wormAttack;
        public AudioClip lasgunFire;
        public AudioClip shieldActivate;
        
        private static PrefabReferences _instance;
        public static PrefabReferences Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PrefabReferences>("PrefabReferences");
                    if (_instance == null)
                    {
                        Debug.LogError("PrefabReferences not found in Resources!");
                    }
                }
                return _instance;
            }
        }
        
        public GameObject GetUnitPrefab(string unitName)
        {
            switch (unitName.ToLower())
            {
                case "atreides trooper":
                case "trooper":
                    return atreidesTrooper;
                case "atreides commando":
                case "commando":
                    return atreidesCommando;
                case "swordmaster":
                    return swordmaster;
                case "ornithopter scout":
                case "scout thopter":
                    return ornithopterScout;
                case "fighter ornithopter":
                case "fighter thopter":
                    return ornithopterFighter;
                case "carryall":
                    return carryall;
                case "harkonnen slave":
                case "slave":
                    return harkonnenSlave;
                case "harkonnen heavy":
                case "heavy infantry":
                    return harkonnenHeavy;
                case "harkonnen elite":
                    return harkonnenElite;
                case "harkonnen tank":
                case "tank":
                    return harkonnenTank;
                case "fremen warrior":
                case "warrior":
                    return fremenWarrior;
                case "fedaykin":
                    return fremenFedaykin;
                case "fremen naib":
                case "naib":
                    return fremenNaib;
                case "sandworm":
                case "shai-hulud":
                    return sandworm;
                default:
                    Debug.LogWarning($"Unknown unit: {unitName}");
                    return atreidesTrooper;
            }
        }
        
        public GameObject GetBuildingPrefab(string buildingName)
        {
            switch (buildingName.ToLower())
            {
                case "command center":
                case "command":
                    return commandCenter;
                case "barracks":
                    return barracks;
                case "spice refinery":
                case "refinery":
                    return spiceRefinery;
                case "windtrap":
                    return windtrap;
                case "ornithopter bay":
                case "thopter bay":
                    return ornithopterBay;
                case "shield generator":
                case "shield":
                    return shieldGenerator;
                case "deathstill":
                    return deathstill;
                case "sietch entrance":
                case "sietch":
                    return sietchEntrance;
                default:
                    Debug.LogWarning($"Unknown building: {buildingName}");
                    return commandCenter;
            }
        }
        
        public GameObject GetEffectPrefab(string effectName)
        {
            switch (effectName.ToLower())
            {
                case "sand explosion":
                case "explosion":
                    return sandExplosion;
                case "spice bloom":
                    return spiceBloom;
                case "shield hit":
                    return shieldHit;
                case "worm emergence":
                    return wormEmergence;
                case "lasgun beam":
                    return lasgunBeam;
                case "voice wave":
                    return voiceWave;
                default:
                    Debug.LogWarning($"Unknown effect: {effectName}");
                    return sandExplosion;
            }
        }
        
        public void PreloadAllPrefabs()
        {
            // Force load all prefabs into memory
            var allPrefabs = new List<GameObject>
            {
                atreidesTrooper, atreidesCommando, swordmaster,
                ornithopterScout, ornithopterFighter, carryall,
                harkonnenSlave, harkonnenHeavy, harkonnenElite, harkonnenTank,
                fremenWarrior, fremenFedaykin, fremenNaib, sandworm,
                commandCenter, barracks, spiceRefinery, windtrap,
                ornithopterBay, shieldGenerator, deathstill, sietchEntrance,
                sandExplosion, spiceBloom, shieldHit, wormEmergence, lasgunBeam, voiceWave
            };
            
            foreach (var prefab in allPrefabs)
            {
                if (prefab != null)
                {
                    // Instantiate and destroy to force loading
                    var temp = Instantiate(prefab);
                    DestroyImmediate(temp);
                }
            }
        }
    }
}
