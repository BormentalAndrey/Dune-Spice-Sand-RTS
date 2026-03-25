using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Integration
{
    /// <summary>
    /// Achievement system with Google Play Games integration
    /// References: Dune lore-based achievements
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }
        
        [Header("Google Play Settings")]
        public bool useGooglePlay = true;
        public string leaderboardId = "CgkI__example_leaderboard";
        
        [Header("Achievements")]
        public List<Achievement> achievements = new List<Achievement>();
        
        [System.Serializable]
        public class Achievement
        {
            public string id;
            public string title;
            public string description;
            public int progressTarget;
            public int currentProgress;
            public bool isUnlocked;
            public string bookReference;
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
            
            InitializeAchievements();
        }
        
        private void InitializeAchievements()
        {
            // Dune lore achievements
            achievements.Add(new Achievement
            {
                id = "CgkI__spice_must_flow",
                title = "The Spice Must Flow",
                description = "Harvest 10,000 Spice",
                progressTarget = 10000,
                bookReference = "Dune, Book I, Chapter 1"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__worm_rider",
                title = "Rider of Shai-Hulud",
                description = "Successfully ride a sandworm",
                progressTarget = 1,
                bookReference = "Dune, Book I, Chapter 35"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__kwisatz_haderach",
                title = "Kwisatz Haderach",
                description = "Complete the Water of Life ritual",
                progressTarget = 1,
                bookReference = "Dune, Book I, Chapter 36"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__fremen_legend",
                title = "Fremen Legend",
                description = "Achieve max reputation with Fremen",
                progressTarget = 100,
                bookReference = "Dune, Book II, Chapter 20"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__shield_explosion",
                title = "Nuclear Reaction",
                description = "Survive a lasgun-shield explosion",
                progressTarget = 1,
                bookReference = "Dune, Book I, Chapter 15"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__voice_master",
                title = "The Voice",
                description = "Use Bene Gesserit Voice 100 times",
                progressTarget = 100,
                bookReference = "Dune, Book I, Chapter 1"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__jihad_leader",
                title = "Muad'Dib's Jihad",
                description = "Complete the Jihad campaign path",
                progressTarget = 1,
                bookReference = "Dune Messiah"
            });
            
            achievements.Add(new Achievement
            {
                id = "CgkI__god_emperor",
                title = "God Emperor",
                description = "Complete Leto II transformation path",
                progressTarget = 1,
                bookReference = "God Emperor of Dune"
            });
        }
        
        public void UpdateAchievement(string achievementId, int progress)
        {
            Achievement achievement = achievements.Find(a => a.id == achievementId);
            if (achievement == null || achievement.isUnlocked) return;
            
            achievement.currentProgress = Mathf.Min(achievement.currentProgress + progress, achievement.progressTarget);
            
            if (achievement.currentProgress >= achievement.progressTarget)
            {
                UnlockAchievement(achievement);
            }
            
            // Update Google Play
            if (useGooglePlay)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                Social.ReportProgress(achievementId, achievement.currentProgress, success => {
                    Debug.Log($"Achievement {achievement.title}: {success}");
                });
                #endif
            }
        }
        
        private void UnlockAchievement(Achievement achievement)
        {
            if (achievement.isUnlocked) return;
            
            achievement.isUnlocked = true;
            Debug.Log($"Achievement Unlocked: {achievement.title} - {achievement.bookReference}");
            
            // Show unlock popup
            UIManager.Instance?.ShowAchievementUnlock(achievement.title);
            
            // Play sound
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null)
                audio.Play();
            
            // Google Play unlock
            if (useGooglePlay)
            {
                #if UNITY_ANDROID && !UNITY_EDITOR
                Social.ReportProgress(achievement.id, 100.0f, success => {});
                #endif
            }
        }
        
        public void UpdateLeaderboardScore(int score)
        {
            if (!useGooglePlay) return;
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            Social.ReportScore(score, leaderboardId, success => {
                Debug.Log($"Leaderboard score updated: {success}");
            });
            #endif
        }
        
        // Game event triggers
        public void OnSpiceHarvested(float amount)
        {
            UpdateAchievement("CgkI__spice_must_flow", (int)amount);
        }
        
        public void OnWormRidden()
        {
            UpdateAchievement("CgkI__worm_rider", 1);
        }
        
        public void OnWaterOfLifeCompleted()
        {
            UpdateAchievement("CgkI__kwisatz_haderach", 1);
        }
        
        public void OnFremenReputationIncreased(int amount)
        {
            UpdateAchievement("CgkI__fremen_legend", amount);
        }
        
        public void OnShieldExplosionSurvived()
        {
            UpdateAchievement("CgkI__shield_explosion", 1);
        }
        
        public void OnVoiceUsed()
        {
            UpdateAchievement("CgkI__voice_master", 1);
        }
        
        public void OnJihadCompleted()
        {
            UpdateAchievement("CgkI__jihad_leader", 1);
        }
        
        public void OnGodEmperorPath()
        {
            UpdateAchievement("CgkI__god_emperor", 1);
        }
    }
}
