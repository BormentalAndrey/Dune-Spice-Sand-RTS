using UnityEngine;
using System.Collections;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Manages game time, day/night cycle on Arrakis
    /// References: Dune, Book I, Chapter 4 - Arrakis day/night cycle
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }
        
        [Header("Time Settings")]
        public float gameSpeed = 1f;
        public float realTimeSecondsPerGameHour = 30f;
        
        [Header("Day/Night Cycle")]
        public float currentHour = 0f; // 0-24 hours
        public float dayDuration = 24f;
        public Light sunLight;
        public Gradient lightGradient;
        
        [Header("Arrakis Calendar - Dune, Book I, Chapter 4")]
        public int currentDay = 1;
        public int currentMonth = 1;
        public int currentYear = 10191; // Dune begins in 10,191 A.G.
        
        [Header("Spice Cycle")]
        public float spiceBloomChance = 0.01f;
        public float lastSpiceBloomTime;
        
        private bool isGamePaused = false;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            StartCoroutine(TimeRoutine());
            StartCoroutine(SpiceCycleRoutine());
        }
        
        private IEnumerator TimeRoutine()
        {
            while (true)
            {
                if (!isGamePaused)
                {
                    // Advance time
                    float deltaHours = Time.deltaTime / realTimeSecondsPerGameHour;
                    deltaHours *= gameSpeed;
                    currentHour += deltaHours;
                    
                    if (currentHour >= dayDuration)
                    {
                        currentHour -= dayDuration;
                        AdvanceDay();
                    }
                    
                    // Update lighting
                    UpdateLighting();
                }
                
                yield return null;
            }
        }
        
        private void UpdateLighting()
        {
            if (sunLight == null) return;
            
            // Calculate light intensity based on hour
            float timeOfDay = currentHour / dayDuration;
            float intensity = Mathf.Sin(timeOfDay * Mathf.PI);
            intensity = Mathf.Clamp(intensity, 0.2f, 1f);
            
            sunLight.intensity = intensity;
            
            // Apply gradient color
            if (lightGradient != null)
            {
                sunLight.color = lightGradient.Evaluate(timeOfDay);
            }
            
            // Rotate sun based on time
            float sunAngle = timeOfDay * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);
        }
        
        private void AdvanceDay()
        {
            currentDay++;
            
            // Arrakis calendar - 30-day months
            if (currentDay > 30)
            {
                currentDay = 1;
                currentMonth++;
                
                if (currentMonth > 12)
                {
                    currentMonth = 1;
                    currentYear++;
                }
            }
            
            // Daily events
            OnNewDay();
        }
        
        private void OnNewDay()
        {
            // Daily spice production from harvesters
            // Daily water collection from windtraps
            
            // Check for spice bloom
            if (Random.value < spiceBloomChance)
            {
                TriggerSpiceBloom();
            }
            
            // Check for sandstorm chance
            if (Random.value < 0.3f)
            {
                TriggerSandstorm();
            }
            
            // Update UI
            UIManager.Instance?.UpdateDate(currentDay, currentMonth, currentYear);
        }
        
        private IEnumerator SpiceCycleRoutine()
        {
            // Spice cycle - Dune ecology
            while (true)
            {
                yield return new WaitForSeconds(300f); // Every 5 minutes game time
                
                // Regenerate depleted spice fields
                SpiceField[] spiceFields = FindObjectsOfType<SpiceField>();
                foreach (var field in spiceFields)
                {
                    if (field.isDepleted && Time.time > lastSpiceBloomTime + 60f)
                    {
                        field.spiceAmount += field.maxSpice * 0.1f;
                        if (field.spiceAmount > field.maxSpice * 0.1f)
                        {
                            field.isDepleted = false;
                            field.ReactivateField();
                        }
                    }
                }
            }
        }
        
        private void TriggerSpiceBloom()
        {
            // Spice bloom event - new spice appears
            Debug.Log("Spice Bloom! New spice fields appearing.");
            
            // Create new spice field at random location
            SandTerrain terrain = FindObjectOfType<SandTerrain>();
            if (terrain != null)
            {
                Vector3 spawnPos = terrain.GetRandomSandPosition();
                terrain.SpawnSpiceField(spawnPos);
            }
            
            lastSpiceBloomTime = Time.time;
        }
        
        private void TriggerSandstorm()
        {
            // Create sandstorm at random edge of map
            SandTerrain terrain = FindObjectOfType<SandTerrain>();
            if (terrain != null && sandstormPrefab != null)
            {
                Vector3 spawnPos = GetRandomEdgePosition();
                Vector3 direction = GetRandomDirection();
                
                Sandstorm storm = Instantiate(sandstormPrefab, spawnPos, Quaternion.identity).GetComponent<Sandstorm>();
                if (storm != null)
                {
                    storm.Initialize(Random.Range(50f, 150f), direction, spawnPos);
                }
            }
        }
        
        private Vector3 GetRandomEdgePosition()
        {
            // Return random position on map edge
            float x = Random.Range(0, 100);
            float z = Random.Range(0, 100);
            
            if (Random.value < 0.5f)
                x = Random.value < 0.5f ? 0 : 100;
            else
                z = Random.value < 0.5f ? 0 : 100;
                
            return new Vector3(x, 0, z);
        }
        
        private Vector3 GetRandomDirection()
        {
            Vector2 dir2D = Random.insideUnitCircle.normalized;
            return new Vector3(dir2D.x, 0, dir2D.y);
        }
        
        public void SetGameSpeed(float speed)
        {
            gameSpeed = Mathf.Clamp(speed, 0.5f, 3f);
            Time.timeScale = gameSpeed;
        }
        
        public void PauseGame(bool pause)
        {
            isGamePaused = pause;
            Time.timeScale = pause ? 0f : gameSpeed;
        }
        
        public string GetCurrentTimeString()
        {
            int hour = Mathf.FloorToInt(currentHour);
            int minute = Mathf.FloorToInt((currentHour - hour) * 60);
            return $"{hour:00}:{minute:00}";
        }
        
        public GameObject sandstormPrefab;
    }
}
