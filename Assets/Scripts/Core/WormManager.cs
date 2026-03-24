using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dune.SpiceAndSand.Ecology
{
    /// <summary>
    /// Manages sandworm behavior - Book-accurate vibration attraction
    /// References: Dune, Book I, Chapter 3, 18, 35
    /// </summary>
    public class WormManager : MonoBehaviour
    {
        public static WormManager Instance { get; private set; }
        
        [Header("Worm Settings")]
        public GameObject sandwormPrefab;
        public int maxActiveWorms = 3;
        public float wormSpawnDelay = 30f;
        public float vibrationDecayRate = 2f;
        public float wormSpawnThreshold = 5f;
        
        [Header("Vibration Map")]
        private Dictionary<Vector3, float> vibrationSources = new Dictionary<Vector3, float>();
        private List<SandwormAI> activeWorms = new List<SandwormAI>();
        
        [Header("Thumper Mechanic - Dune, Book I, Chapter 18")]
        public bool isThumperActive = false;
        public Vector3 thumperPosition;
        public float thumperDuration = 10f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            UpdateVibrationDecay();
            CheckWormSpawn();
        }
        
        /// <summary>
        /// Register vibration at a position - Dune, Book I, Chapter 3
        /// "Walk without rhythm, and you won't attract the worm"
        /// </summary>
        public void RegisterVibration(float intensity, Vector3 position)
        {
            if (vibrationSources.ContainsKey(position))
            {
                vibrationSources[position] = Mathf.Min(vibrationSources[position] + intensity, 10f);
            }
            else
            {
                vibrationSources[position] = intensity;
            }
            
            // Remove old sources after a while
            StartCoroutine(RemoveVibrationAfterDelay(position, 5f));
        }
        
        private IEnumerator RemoveVibrationAfterDelay(Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (vibrationSources.ContainsKey(position))
            {
                vibrationSources.Remove(position);
            }
        }
        
        private void UpdateVibrationDecay()
        {
            List<Vector3> toRemove = new List<Vector3>();
            foreach (var source in vibrationSources)
            {
                float newIntensity = source.Value - vibrationDecayRate * Time.deltaTime;
                if (newIntensity <= 0)
                {
                    toRemove.Add(source.Key);
                }
                else
                {
                    vibrationSources[source.Key] = newIntensity;
                }
            }
            
            foreach (var pos in toRemove)
            {
                vibrationSources.Remove(pos);
            }
        }
        
        private void CheckWormSpawn()
        {
            if (activeWorms.Count >= maxActiveWorms) return;
            
            // Find highest vibration point
            float highestIntensity = 0;
            Vector3 spawnPosition = Vector3.zero;
            
            foreach (var source in vibrationSources)
            {
                if (source.Value > highestIntensity)
                {
                    highestIntensity = source.Value;
                    spawnPosition = source.Key;
                }
            }
            
            if (highestIntensity >= wormSpawnThreshold)
            {
                SpawnWorm(spawnPosition);
            }
        }
        
        private void SpawnWorm(Vector3 position)
        {
            if (sandwormPrefab != null)
            {
                GameObject wormObj = Instantiate(sandwormPrefab, position, Quaternion.identity);
                SandwormAI worm = wormObj.GetComponent<SandwormAI>();
                if (worm != null)
                {
                    activeWorms.Add(worm);
                    worm.OnDestroyed += () => activeWorms.Remove(worm);
                }
            }
        }
        
        /// <summary>
        /// Deploy thumper to distract worms - Dune, Book I, Chapter 18
        /// </summary>
        public void DeployThumper(Vector3 position)
        {
            isThumperActive = true;
            thumperPosition = position;
            
            // Thumper attracts all worms to that position
            foreach (var worm in activeWorms)
            {
                worm.SetTargetPosition(position);
            }
            
            StartCoroutine(ThumperDuration());
        }
        
        private IEnumerator ThumperDuration()
        {
            yield return new WaitForSeconds(thumperDuration);
            isThumperActive = false;
        }
        
        public bool IsPositionSafeFromWorms(Vector3 position, float radius)
        {
            // Check if any worm is within radius
            foreach (var worm in activeWorms)
            {
                if (worm != null && Vector3.Distance(worm.transform.position, position) < radius)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
