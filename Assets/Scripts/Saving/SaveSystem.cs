using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Saving
{
    /// <summary>
    /// Save system for game progress and settings
    /// Supports cloud save via Google Play
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }
        
        [Header("Save Settings")]
        public string saveFileName = "dune_save.dat";
        public bool useCloudSave = false;
        
        [Header("Auto-Save")]
        public float autoSaveInterval = 300f; // 5 minutes
        public bool autoSaveEnabled = true;
        
        private SaveData currentSave;
        private float lastAutoSaveTime;
        
        [System.Serializable]
        public class SaveData
        {
            // Game progress
            public int currentMission;
            public bool[] missionCompleted;
            public GameManager.Faction playerFaction;
            
            // Resources
            public float spice;
            public float water;
            public float jihadMeter;
            public float terraformingProgress;
            
            // Date
            public int currentDay;
            public int currentMonth;
            public int currentYear;
            
            // Achievements
            public List<string> unlockedAchievements;
            
            // Statistics
            public int totalSpiceHarvested;
            public int totalEnemiesKilled;
            public int wormsRidden;
            public int voiceCommandsUsed;
            
            // Building and unit counts
            public Dictionary<string, int> buildingsBuilt;
            public Dictionary<string, int> unitsTrained;
            
            // Timestamp
            public DateTime lastSaveTime;
            public float totalPlayTime;
        }
        
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
            LoadGame();
            lastAutoSaveTime = Time.time;
        }
        
        private void Update()
        {
            if (autoSaveEnabled && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveGame();
                lastAutoSaveTime = Time.time;
            }
        }
        
        public void SaveGame()
        {
            currentSave = new SaveData();
            
            // Save progress
            currentSave.currentMission = CampaignManager.Instance?.currentMission ?? 1;
            currentSave.missionCompleted = CampaignManager.Instance?.missionCompleted;
            currentSave.playerFaction = GameManager.Instance?.playerFaction ?? GameManager.Faction.Atreides;
            
            // Save resources
            currentSave.spice = GameManager.Instance?.spice ?? 0;
            currentSave.water = GameManager.Instance?.water ?? 0;
            currentSave.jihadMeter = GameManager.Instance?.jihadMeter ?? 0;
            currentSave.terraformingProgress = GameManager.Instance?.terraformingProgress ?? 0;
            
            // Save date
            currentSave.currentDay = TimeManager.Instance?.currentDay ?? 1;
            currentSave.currentMonth = TimeManager.Instance?.currentMonth ?? 1;
            currentSave.currentYear = TimeManager.Instance?.currentYear ?? 10191;
            
            // Save timestamp
            currentSave.lastSaveTime = DateTime.Now;
            currentSave.totalPlayTime = Time.time;
            
            // Serialize and save to file
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(filePath, FileMode.Create);
                formatter.Serialize(stream, currentSave);
                stream.Close();
                
                Debug.Log($"Game saved to {filePath}");
                
                // Cloud save
                if (useCloudSave)
                {
                    StartCoroutine(CloudSave());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }
        
        public void LoadGame()
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(filePath, FileMode.Open);
                    currentSave = (SaveData)formatter.Deserialize(stream);
                    stream.Close();
                    
                    // Apply save data
                    ApplySaveData();
                    
                    Debug.Log($"Game loaded from {filePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Load failed: {e.Message}");
                    CreateNewSave();
                }
            }
            else
            {
                Debug.Log("No save file found, creating new save");
                CreateNewSave();
            }
        }
        
        private void ApplySaveData()
        {
            // Apply to game managers
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentMissionIndex = currentSave.currentMission;
                GameManager.Instance.spice = currentSave.spice;
                GameManager.Instance.water = currentSave.water;
                GameManager.Instance.jihadMeter = currentSave.jihadMeter;
                GameManager.Instance.terraformingProgress = currentSave.terraformingProgress;
                GameManager.Instance.playerFaction = currentSave.playerFaction;
            }
            
            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.currentMission = currentSave.currentMission;
                CampaignManager.Instance.missionCompleted = currentSave.missionCompleted;
            }
            
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.currentDay = currentSave.currentDay;
                TimeManager.Instance.currentMonth = currentSave.currentMonth;
                TimeManager.Instance.currentYear = currentSave.currentYear;
            }
        }
        
        private void CreateNewSave()
        {
            currentSave = new SaveData
            {
                currentMission = 1,
                missionCompleted = new bool[19],
                playerFaction = GameManager.Faction.Atreides,
                spice = 0,
                water = 100,
                currentDay = 1,
                currentMonth = 1,
                currentYear = 10191,
                unlockedAchievements = new List<string>(),
                buildingsBuilt = new Dictionary<string, int>(),
                unitsTrained = new Dictionary<string, int>()
            };
            
            SaveGame();
        }
        
        private IEnumerator CloudSave()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // TODO: Implement Google Play Cloud Save
            yield return null;
            #else
            yield return null;
            #endif
        }
        
        public void DeleteSave()
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("Save file deleted");
            }
            
            CreateNewSave();
        }
        
        public bool HasSaveFile()
        {
            string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
            return File.Exists(filePath);
        }
        
        public SaveData GetCurrentSave()
        {
            return currentSave;
        }
        
        // Statistics tracking
        public void RecordSpiceHarvested(int amount)
        {
            if (currentSave != null)
            {
                currentSave.totalSpiceHarvested += amount;
                SaveGame();
            }
        }
        
        public void RecordEnemyKilled()
        {
            if (currentSave != null)
            {
                currentSave.totalEnemiesKilled++;
                SaveGame();
            }
        }
        
        public void RecordWormRidden()
        {
            if (currentSave != null)
            {
                currentSave.wormsRidden++;
                SaveGame();
            }
        }
        
        public void RecordVoiceCommand()
        {
            if (currentSave != null)
            {
                currentSave.voiceCommandsUsed++;
                SaveGame();
            }
        }
        
        public void RecordBuildingBuilt(string buildingType)
        {
            if (currentSave != null)
            {
                if (!currentSave.buildingsBuilt.ContainsKey(buildingType))
                    currentSave.buildingsBuilt[buildingType] = 0;
                    
                currentSave.buildingsBuilt[buildingType]++;
                SaveGame();
            }
        }
        
        public void RecordUnitTrained(string unitType)
        {
            if (currentSave != null)
            {
                if (!currentSave.unitsTrained.ContainsKey(unitType))
                    currentSave.unitsTrained[unitType] = 0;
                    
                currentSave.unitsTrained[unitType]++;
                SaveGame();
            }
        }
    }
}
