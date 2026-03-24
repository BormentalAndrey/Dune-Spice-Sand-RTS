
---

## Core C# Scripts

### 1. GameManager.cs (Core)

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Main game controller - Dune: Chapter 1, "The Butlerian Jihad"
    /// Reference: Dune, Book I, "The Delve"
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public GameState currentState = GameState.MainMenu;
        public Faction playerFaction;
        public int currentMissionIndex = 1;
        
        [Header("Resources")]
        public float spice = 0f;
        public float water = 100f;
        public float spiceCapacity = 1000f;
        public float waterCapacity = 500f;
        
        [Header("Jihad System - Dune Messiah reference")]
        public float jihadMeter = 0f; // 0-100, triggers Paul's path
        public bool paulPathChosen = false;
        
        [Header("Ecology")]
        public float terraformingProgress = 0f; // Fremen terraforming goal
        
        [Header("Events")]
        public Action<GameState> OnGameStateChanged;
        public Action<float, float> OnResourcesUpdated;
        public Action<float> OnJihadMeterChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            // References: Dune, Book I, "The Delve" - Initial spice holdings
            spice = 0f;
            water = 100f;
            jihadMeter = 0f;
            terraformingProgress = 0f;
            
            OnResourcesUpdated?.Invoke(spice, water);
        }
        
        public void AddSpice(float amount)
        {
            spice = Mathf.Min(spice + amount, spiceCapacity);
            OnResourcesUpdated?.Invoke(spice, water);
            
            // Spice attracts worms - Dune, Book I, Chapter 3
            WormManager.Instance?.RegisterVibration(amount / 10f, transform.position);
        }
        
        public bool ConsumeSpice(float amount)
        {
            if (spice >= amount)
            {
                spice -= amount;
                OnResourcesUpdated?.Invoke(spice, water);
                return true;
            }
            return false;
        }
        
        public void AddWater(float amount)
        {
            water = Mathf.Min(water + amount, waterCapacity);
            OnResourcesUpdated?.Invoke(spice, water);
        }
        
        public bool ConsumeWater(float amount)
        {
            if (water >= amount)
            {
                water -= amount;
                OnResourcesUpdated?.Invoke(spice, water);
                return true;
            }
            return false;
        }
        
        public void AddJihadProgress(float amount)
        {
            jihadMeter = Mathf.Min(jihadMeter + amount, 100f);
            OnJihadMeterChanged?.Invoke(jihadMeter);
            
            // Dune Messiah: Jihad threshold
            if (jihadMeter >= 80f && !paulPathChosen)
            {
                TriggerJihadDecision();
            }
        }
        
        private void TriggerJihadDecision()
        {
            // Show decision UI - Paul's Jihad path
            UIManager.Instance?.ShowJihadDecision();
        }
        
        public void LoadMission(int missionIndex)
        {
            currentMissionIndex = missionIndex;
            SceneManager.LoadScene($"Campaign_Mission{missionIndex}");
        }
        
        public void LoadSkirmish()
        {
            SceneManager.LoadScene("Skirmish");
        }
        
        public enum GameState
        {
            MainMenu,
            Campaign,
            Skirmish,
            Paused,
            GameOver
        }
        
        public enum Faction
        {
            Atreides,
            Harkonnen,
            Fremen
        }
    }
}
