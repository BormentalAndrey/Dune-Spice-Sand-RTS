using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Campaign
{
    /// <summary>
    /// Mission objective tracking system
    /// References: Dune campaign structure
    /// </summary>
    public class MissionObjective : MonoBehaviour
    {
        [Header("Mission Info")]
        public int missionId;
        public string missionName;
        public List<Objective> objectives = new List<Objective>();
        
        [Header("Mission Events")]
        public Action onMissionComplete;
        public Action<Objective> onObjectiveComplete;
        
        [Header("UI")]
        public GameObject objectivePanel;
        public UnityEngine.UI.Text objectiveText;
        
        private bool missionCompleted = false;
        
        [System.Serializable]
        public class Objective
        {
            public string description;
            public ObjectiveType type;
            public float targetAmount;
            public float currentAmount;
            public bool isComplete;
            public string bookReference;
            
            public enum ObjectiveType
            {
                HarvestSpice,
                BuildBuilding,
                TrainUnits,
                DestroyEnemies,
                SurviveTime,
                ReachLocation,
                CollectWater,
                RideWorm,
                CompleteRitual
            }
        }
        
        private void Start()
        {
            StartCoroutine(UpdateObjectiveUI());
            StartCoroutine(MonitorObjectives());
        }
        
        private IEnumerator MonitorObjectives()
        {
            while (!missionCompleted)
            {
                UpdateAllObjectives();
                CheckMissionComplete();
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void UpdateAllObjectives()
        {
            foreach (var objective in objectives)
            {
                if (objective.isComplete) continue;
                
                switch (objective.type)
                {
                    case Objective.ObjectiveType.HarvestSpice:
                        objective.currentAmount = GameManager.Instance.spice;
                        break;
                        
                    case Objective.ObjectiveType.BuildBuilding:
                        objective.currentAmount = CountBuildings();
                        break;
                        
                    case Objective.ObjectiveType.TrainUnits:
                        objective.currentAmount = CountUnits();
                        break;
                        
                    case Objective.ObjectiveType.DestroyEnemies:
                        objective.currentAmount = GetEnemiesDestroyed();
                        break;
                        
                    case Objective.ObjectiveType.SurviveTime:
                        objective.currentAmount += Time.deltaTime;
                        break;
                        
                    case Objective.ObjectiveType.ReachLocation:
                        objective.currentAmount = CheckLocationReached() ? 1 : 0;
                        break;
                }
                
                if (objective.currentAmount >= objective.targetAmount && !objective.isComplete)
                {
                    CompleteObjective(objective);
                }
            }
        }
        
        private void CompleteObjective(Objective objective)
        {
            objective.isComplete = true;
            onObjectiveComplete?.Invoke(objective);
            
            Debug.Log($"Objective Complete: {objective.description} - {objective.bookReference}");
            
            // Play completion sound
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null)
                audio.Play();
        }
        
        private void CheckMissionComplete()
        {
            bool allComplete = true;
            foreach (var objective in objectives)
            {
                if (!objective.isComplete)
                {
                    allComplete = false;
                    break;
                }
            }
            
            if (allComplete && !missionCompleted)
            {
                CompleteMission();
            }
        }
        
        private void CompleteMission()
        {
            missionCompleted = true;
            onMissionComplete?.Invoke();
            
            CampaignManager.Instance.CompleteMission();
            
            // Show victory screen
            UIManager.Instance.ShowMissionComplete(missionId + 1);
        }
        
        private IEnumerator UpdateObjectiveUI()
        {
            while (true)
            {
                if (objectiveText != null)
                {
                    string text = "";
                    foreach (var objective in objectives)
                    {
                        text += $"{(objective.isComplete ? "✓" : "○")} {objective.description}";
                        
                        if (objective.type != Objective.ObjectiveType.SurviveTime)
                        {
                            text += $" ({objective.currentAmount}/{objective.targetAmount})";
                        }
                        else
                        {
                            int remaining = Mathf.CeilToInt(objective.targetAmount - objective.currentAmount);
                            text += $" ({remaining}s remaining)";
                        }
                        
                        text += "\n";
                    }
                    objectiveText.text = text;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private float CountBuildings()
        {
            BuildingBase[] buildings = FindObjectsOfType<BuildingBase>();
            return buildings.Length;
        }
        
        private float CountUnits()
        {
            UnitBase[] units = FindObjectsOfType<UnitBase>();
            return units.Length;
        }
        
        private float GetEnemiesDestroyed()
        {
            // Track destroyed enemies
            return PlayerPrefs.GetFloat($"Mission{missionId}_Kills", 0f);
        }
        
        private bool CheckLocationReached()
        {
            // Check if player reached target location
            return false; // Implement location check
        }
        
        public void RegisterEnemyKilled()
        {
            float kills = PlayerPrefs.GetFloat($"Mission{missionId}_Kills", 0f);
            kills++;
            PlayerPrefs.SetFloat($"Mission{missionId}_Kills", kills);
        }
        
        // Mission 1 specific objectives
        public static List<Objective> GetMission1Objectives()
        {
            return new List<Objective>
            {
                new Objective
                {
                    description = "Establish Command Center",
                    type = Objective.ObjectiveType.BuildBuilding,
                    targetAmount = 1,
                    bookReference = "Dune, Book I, Chapter 4"
                },
                new Objective
                {
                    description = "Construct Spice Refinery",
                    type = Objective.ObjectiveType.BuildBuilding,
                    targetAmount = 1,
                    bookReference = "Dune, Book I, Chapter 4"
                },
                new Objective
                {
                    description = "Harvest 500 Spice",
                    type = Objective.ObjectiveType.HarvestSpice,
                    targetAmount = 500,
                    bookReference = "Dune, Book I, Chapter 4"
                },
                new Objective
                {
                    description = "Survive Worm Attack",
                    type = Objective.ObjectiveType.SurviveTime,
                    targetAmount = 60,
                    bookReference = "Dune, Book I, Chapter 3"
                }
            };
        }
        
        // Mission 2 objectives
        public static List<Objective> GetMission2Objectives()
        {
            return new List<Objective>
            {
                new Objective
                {
                    description = "Train 3 Scout Units",
                    type = Objective.ObjectiveType.TrainUnits,
                    targetAmount = 3,
                    bookReference = "Dune, Book I, Chapter 5"
                },
                new Objective
                {
                    description = "Discover Fremen Sietch",
                    type = Objective.ObjectiveType.ReachLocation,
                    targetAmount = 1,
                    bookReference = "Dune, Book I, Chapter 25"
                },
                new Objective
                {
                    description = "Establish Contact with Fremen",
                    type = Objective.ObjectiveType.SurviveTime,
                    targetAmount = 30,
                    bookReference = "Dune, Book I, Chapter 25"
                }
            };
        }
        
        // Mission 3 objectives
        public static List<Objective> GetMission3Objectives()
        {
            return new List<Objective>
            {
                new Objective
                {
                    description = "Defend Palace for 10 Minutes",
                    type = Objective.ObjectiveType.SurviveTime,
                    targetAmount = 600,
                    bookReference = "Dune, Book I, Chapter 15"
                },
                new Objective
                {
                    description = "Escape with Paul and Jessica",
                    type = Objective.ObjectiveType.ReachLocation,
                    targetAmount = 1,
                    bookReference = "Dune, Book I, Chapter 17"
                },
                new Objective
                {
                    description = "Reach Fremen Territory",
                    type = Objective.ObjectiveType.ReachLocation,
                    targetAmount = 1,
                    bookReference = "Dune, Book I, Chapter 18"
                }
            };
        }
    }
}
