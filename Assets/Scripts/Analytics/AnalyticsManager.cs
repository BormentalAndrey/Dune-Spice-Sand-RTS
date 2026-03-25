using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Analytics
{
    /// <summary>
    /// Game analytics for player behavior and balance
    /// References: Dune gameplay data collection
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }
        
        [Header("Session")]
        public float sessionStartTime;
        public int sessionNumber;
        
        [Header("Events")]
        public Queue<AnalyticsEvent> eventQueue = new Queue<AnalyticsEvent>();
        public float flushInterval = 30f;
        
        [System.Serializable]
        public class AnalyticsEvent
        {
            public string eventName;
            public Dictionary<string, object> parameters;
            public DateTime timestamp;
            
            public AnalyticsEvent(string name, Dictionary<string, object> data)
            {
                eventName = name;
                parameters = data ?? new Dictionary<string, object>();
                timestamp = DateTime.UtcNow;
            }
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
            
            InitializeAnalytics();
        }
        
        private void Start()
        {
            StartSession();
            StartCoroutine(FlushEventsRoutine());
        }
        
        private void InitializeAnalytics()
        {
            sessionNumber = PlayerPrefs.GetInt("analytics_session_count", 0) + 1;
            PlayerPrefs.SetInt("analytics_session_count", sessionNumber);
        }
        
        private void StartSession()
        {
            sessionStartTime = Time.time;
            
            // Log session start
            TrackEvent("session_start", new Dictionary<string, object>
            {
                { "session_number", sessionNumber },
                { "game_version", Application.version },
                { "platform", Application.platform.ToString() },
                { "device_model", SystemInfo.deviceModel },
                { "os_version", SystemInfo.operatingSystem }
            });
        }
        
        private IEnumerator FlushEventsRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(flushInterval);
                FlushEvents();
            }
        }
        
        private void FlushEvents()
        {
            if (eventQueue.Count == 0) return;
            
            List<AnalyticsEvent> batch = new List<AnalyticsEvent>();
            while (eventQueue.Count > 0 && batch.Count < 20)
            {
                batch.Add(eventQueue.Dequeue());
            }
            
            StartCoroutine(SendEventsToServer(batch));
        }
        
        private IEnumerator SendEventsToServer(List<AnalyticsEvent> events)
        {
            // In production, send to analytics server (Google Analytics, Firebase, etc.)
            yield return null;
        }
        
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            eventQueue.Enqueue(new AnalyticsEvent(eventName, parameters));
            Debug.Log($"Analytics: {eventName}");
        }
        
        public void TrackResourceEvent(string resourceType, float amount, string action)
        {
            TrackEvent("resource_" + action, new Dictionary<string, object>
            {
                { "type", resourceType },
                { "amount", amount },
                { "total_" + resourceType, resourceType == "spice" ? GameManager.Instance.spice : GameManager.Instance.water }
            });
        }
        
        public void TrackUnitEvent(string unitName, string action, int count = 1)
        {
            TrackEvent("unit_" + action, new Dictionary<string, object>
            {
                { "unit", unitName },
                { "count", count },
                { "faction", GameManager.Instance.playerFaction.ToString() }
            });
        }
        
        public void TrackBuildingEvent(string buildingName, string action)
        {
            TrackEvent("building_" + action, new Dictionary<string, object>
            {
                { "building", buildingName },
                { "faction", GameManager.Instance.playerFaction.ToString() }
            });
        }
        
        public void TrackCombatEvent(string attacker, string defender, bool victory)
        {
            TrackEvent("combat", new Dictionary<string, object>
            {
                { "attacker", attacker },
                { "defender", defender },
                { "victory", victory }
            });
        }
        
        public void TrackWormEvent(string eventType, float vibrationIntensity)
        {
            TrackEvent("worm_" + eventType, new Dictionary<string, object>
            {
                { "intensity", vibrationIntensity }
            });
        }
        
        public void TrackMissionEvent(int missionId, bool completed, float completionTime)
        {
            TrackEvent("mission_" + (completed ? "complete" : "fail"), new Dictionary<string, object>
            {
                { "mission_id", missionId },
                { "time", completionTime },
                { "difficulty", 1 } // Add difficulty when implemented
            });
        }
        
        public void TrackAchievementEvent(string achievementId)
        {
            TrackEvent("achievement_unlocked", new Dictionary<string, object>
            {
                { "achievement", achievementId }
            });
        }
        
        public void TrackPurchaseEvent(string productId, float price)
        {
            TrackEvent("purchase", new Dictionary<string, object>
            {
                { "product", productId },
                { "price", price },
                { "currency", "USD" }
            });
        }
        
        private void OnApplicationQuit()
        {
            // End session
            float sessionDuration = Time.time - sessionStartTime;
            
            TrackEvent("session_end", new Dictionary<string, object>
            {
                { "duration", sessionDuration },
                { "session_number", sessionNumber }
            });
            
            // Flush remaining events
            FlushEvents();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Session interrupted
                TrackEvent("session_pause", new Dictionary<string, object>
                {
                    { "duration", Time.time - sessionStartTime }
                });
            }
            else
            {
                // Session resumed
                TrackEvent("session_resume", null);
                sessionStartTime = Time.time;
            }
        }
    }
}
