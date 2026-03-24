using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Mobile performance optimization for 60 FPS target
    /// Dynamic LOD, object pooling, memory management
    /// </summary>
    public class PerformanceOptimizer : MonoBehaviour
    {
        [Header("Quality Settings")]
        public int targetFPS = 60;
        public QualityLevel currentQuality = QualityLevel.Medium;
        
        [Header("Object Pooling")]
        public int maxPoolSize = 100;
        private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();
        
        [Header("LOD Settings")]
        public float lodDistance = 50f;
        public float cullingDistance = 100f;
        public int maxVisibleUnits = 50;
        
        [Header("Dynamic Resolution")]
        public bool enableDynamicResolution = true;
        public float resolutionScale = 1f;
        private float frameTimeAccumulator = 0f;
        private int frameCount = 0;
        
        public enum QualityLevel
        {
            Low,
            Medium,
            High,
            Ultra
        }
        
        private void Start()
        {
            Application.targetFrameRate = targetFPS;
            QualitySettings.vSyncCount = 0;
            
            StartCoroutine(PerformanceMonitor());
            ApplyQualitySettings();
        }
        
        private void Update()
        {
            // Dynamic resolution scaling
            if (enableDynamicResolution)
            {
                UpdateDynamicResolution();
            }
            
            // LOD management
            UpdateLOD();
        }
        
        private IEnumerator PerformanceMonitor()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                
                float currentFPS = 1f / Time.deltaTime;
                
                if (currentFPS < targetFPS * 0.8f)
                {
                    // Performance too low - reduce quality
                    ReduceQuality();
                }
                else if (currentFPS > targetFPS * 1.2f && resolutionScale < 1f)
                {
                    // Performance good - increase resolution
                    IncreaseResolution();
                }
            }
        }
        
        private void UpdateDynamicResolution()
        {
            frameTimeAccumulator += Time.deltaTime;
            frameCount++;
            
            if (frameTimeAccumulator >= 1f)
            {
                float avgFrameTime = frameTimeAccumulator / frameCount;
                float targetFrameTime = 1f / targetFPS;
                
                if (avgFrameTime > targetFrameTime * 1.2f)
                {
                    // Frame time too high - reduce resolution
                    resolutionScale = Mathf.Max(resolutionScale - 0.05f, 0.5f);
                    ApplyResolutionScale();
                }
                else if (avgFrameTime < targetFrameTime * 0.8f && resolutionScale < 1f)
                {
                    // Frame time low - increase resolution
                    resolutionScale = Mathf.Min(resolutionScale + 0.05f, 1f);
                    ApplyResolutionScale();
                }
                
                frameTimeAccumulator = 0f;
                frameCount = 0;
            }
        }
        
        private void ApplyResolutionScale()
        {
            // Unity's dynamic resolution
            if (ScalableBufferManager.IsDynamicResolutionSupported())
            {
                float widthScale = resolutionScale;
                float heightScale = resolutionScale;
                ScalableBufferManager.ResizeBuffers(widthScale, heightScale);
            }
        }
        
        private void ApplyQualitySettings()
        {
            switch (currentQuality)
            {
                case QualityLevel.Low:
                    QualitySettings.shadowCascades = 0;
                    QualitySettings.shadowDistance = 20f;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.masterTextureLimit = 2;
                    break;
                    
                case QualityLevel.Medium:
                    QualitySettings.shadowCascades = 2;
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.masterTextureLimit = 1;
                    break;
                    
                case QualityLevel.High:
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowDistance = 100f;
                    QualitySettings.antiAliasing = 4;
                    QualitySettings.masterTextureLimit = 0;
                    break;
                    
                case QualityLevel.Ultra:
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowDistance = 150f;
                    QualitySettings.antiAliasing = 8;
                    QualitySettings.masterTextureLimit = 0;
                    break;
            }
        }
        
        private void ReduceQuality()
        {
            if (currentQuality == QualityLevel.Ultra)
                currentQuality = QualityLevel.High;
            else if (currentQuality == QualityLevel.High)
                currentQuality = QualityLevel.Medium;
            else if (currentQuality == QualityLevel.Medium)
                currentQuality = QualityLevel.Low;
                
            ApplyQualitySettings();
            Debug.Log($"Performance reduced to {currentQuality} quality");
        }
        
        private void IncreaseResolution()
        {
            if (resolutionScale < 0.9f)
            {
                resolutionScale += 0.1f;
                ApplyResolutionScale();
            }
        }
        
        private void UpdateLOD()
        {
            // Find all units and apply LOD
            UnitBase[] units = FindObjectsOfType<UnitBase>();
            int visibleCount = 0;
            
            foreach (var unit in units)
            {
                float distance = Vector3.Distance(Camera.main.transform.position, unit.transform.position);
                
                // Culling
                if (distance > cullingDistance)
                {
                    unit.gameObject.SetActive(false);
                    continue;
                }
                else if (!unit.gameObject.activeSelf)
                {
                    unit.gameObject.SetActive(true);
                }
                
                // LOD
                if (distance > lodDistance && visibleCount > maxVisibleUnits)
                {
                    // Reduce update rate
                    unit.enabled = false;
                }
                else
                {
                    unit.enabled = true;
                    visibleCount++;
                }
            }
        }
        
        // Object pooling for units and effects
        public GameObject GetPooledObject(string poolKey, GameObject prefab)
        {
            if (!objectPools.ContainsKey(poolKey))
            {
                objectPools[poolKey] = new Queue<GameObject>();
            }
            
            Queue<GameObject> pool = objectPools[poolKey];
            
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                return Instantiate(prefab);
            }
        }
        
        public void ReturnToPool(string poolKey, GameObject obj)
        {
            if (!objectPools.ContainsKey(poolKey))
            {
                objectPools[poolKey] = new Queue<GameObject>();
            }
            
            obj.SetActive(false);
            objectPools[poolKey].Enqueue(obj);
            
            // Limit pool size
            if (objectPools[poolKey].Count > maxPoolSize)
            {
                GameObject excess = objectPools[poolKey].Dequeue();
                Destroy(excess);
            }
        }
        
        public void ClearAllPools()
        {
            foreach (var pool in objectPools.Values)
            {
                foreach (var obj in pool)
                {
                    Destroy(obj);
                }
            }
            objectPools.Clear();
        }
        
        // Memory management
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Free up memory when app is paused
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
        
        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}
