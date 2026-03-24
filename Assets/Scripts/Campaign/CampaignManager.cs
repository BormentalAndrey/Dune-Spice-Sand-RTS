using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dune.SpiceAndSand.Campaign
{
    /// <summary>
    /// Campaign manager with book-accurate missions
    /// References: Dune, Books I-VI
    /// </summary>
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }
        
        [Header("Campaign Progress")]
        public int currentMission = 1;
        public bool[] missionCompleted = new bool[19]; // 18 missions + 1 index
        
        [Header("Dialogue System")]
        public DialogueSystem dialogueSystem;
        
        [Header("Campaign Data")]
        public List<MissionData> missions;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeMissions();
        }
        
        private void InitializeMissions()
        {
            missions = new List<MissionData>();
            
            // Mission 1: Arrival on Arrakis
            missions.Add(new MissionData
            {
                missionId = 1,
                title = "Arrival on Arrakis",
                description = "Duke Leto Atreides arrives on Arrakis to take control of spice mining.",
                objectives = new List<Objective>
                {
                    new Objective { description = "Establish Command Center", isComplete = false },
                    new Objective { description = "Construct Spice Refinery", isComplete = false },
                    new Objective { description = "Harvest 500 Spice", isComplete = false },
                    new Objective { description = "Survive First Worm Attack", isComplete = false }
                },
                startingResources = new ResourcesData { spice = 0, water = 100 },
                bookReference = "Dune, Book I, Chapter 1-4"
            });
            
            // Mission 2: Desert Reconnaissance
            missions.Add(new MissionData
            {
                missionId = 2,
                title = "Desert Reconnaissance",
                description = "Send scouts into the deep desert to map Fremen territory.",
                objectives = new List<Objective>
                {
                    new Objective { description = "Train 3 Scout Units", isComplete = false },
                    new Objective { description = "Discover Fremen Sietch", isComplete = false },
                    new Objective { description = "Establish Contact with Fremen", isComplete = false }
                },
                startingResources = new ResourcesData { spice = 200, water = 150 },
                bookReference = "Dune, Book I, Chapter 5-8"
            });
            
            // Mission 3: Harkonnen Trap
            missions.Add(new MissionData
            {
                missionId = 3,
                title = "The Harkonnen Trap",
                description = "Baron Harkonnen launches a surprise attack with Imperial Sardaukar.",
                objectives = new List<Objective>
                {
                    new Objective { description = "Defend the Palace for 10 Minutes", isComplete = false },
                    new Objective { description = "Escape with Paul and Jessica", isComplete = false },
                    new Objective { description = "Reach Fremen Territory", isComplete = false }
                },
                startingResources = new ResourcesData { spice = 300, water = 200 },
                bookReference = "Dune, Book I, Chapter 15-17"
            });
            
            // Additional missions continue through the books...
        }
        
        public void StartMission(int missionId)
        {
            currentMission = missionId;
            SceneManager.LoadScene($"Campaign_Mission{missionId}");
        }
        
        public void CompleteMission()
        {
            missionCompleted[currentMission] = true;
            
            // Check for next mission
            if (currentMission < missions.Count)
            {
                UnlockNextMission();
            }
        }
        
        private void UnlockNextMission()
        {
            // Show mission complete UI
            UIManager.Instance?.ShowMissionComplete(currentMission + 1);
        }
        
        public MissionData GetCurrentMission()
        {
            if (currentMission <= missions.Count)
            {
                return missions[currentMission - 1];
            }
            return null;
        }
    }
    
    [System.Serializable]
    public class MissionData
    {
        public int missionId;
        public string title;
        public string description;
        public List<Objective> objectives;
        public ResourcesData startingResources;
        public string bookReference;
        public string cutsceneText;
    }
    
    [System.Serializable]
    public class Objective
    {
        public string description;
        public bool isComplete;
    }
    
    [System.Serializable]
    public class ResourcesData
    {
        public float spice;
        public float water;
    }
}
