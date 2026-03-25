using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Dune.SpiceAndSand.Debugging
{
    /// <summary>
    /// Performance metrics display for debugging and optimization
    /// </summary>
    public class PerformanceMetrics : MonoBehaviour
    {
        [Header("Display")]
        public TextMeshProUGUI fpsText;
        public TextMeshProUGUI memoryText;
        public TextMeshProUGUI drawCallText;
        public TextMeshProUGUI unitCountText;
        public bool showMetrics = false;
        
        [Header("FPS")]
        public float updateInterval = 0.5f;
        private float lastInterval;
        private int frames = 0;
        private float fps;
        
        [Header("Memory")]
        private float memoryUsage;
        
        [Header("Units")]
        private int unitCount;
        private int buildingCount;
        
        [Header("Draw Calls")]
        private int drawCalls;
        
        private void Start()
        {
            lastInterval = Time.realtimeSinceStartup;
            frames = 0;
            
            // Start metrics collection
            StartCoroutine(CollectMetrics());
        }
        
        private void Update()
        {
            frames++;
            
            if (Time.realtimeSinceStartup > lastInterval + updateInterval)
            {
                fps = frames / (Time.realtimeSinceStartup - lastInterval);
                frames = 0;
                lastInterval = Time.realtimeSinceStartup;
                
                UpdateDisplay();
            }
        }
        
        private IEnumerator CollectMetrics()
        {
            while (true)
            {
                // Collect unit count
                unitCount = FindObjectsOfType<UnitBase>().Length;
                buildingCount = FindObjectsOfType<BuildingBase>().Length;
                
                // Collect memory usage
                memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
                
                // Collect draw calls (Unity 2022+)
                drawCalls = UnityEngine.Rendering.StatisticsInfo.drawCalls;
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        private void UpdateDisplay()
        {
            if (!showMetrics) return;
            
            if (fpsText != null)
            {
                Color fpsColor = Color.green;
                if (fps < 30) fpsColor = Color.red;
                else if (fps < 50) fpsColor = Color.yellow;
                
                fpsText.text = $"FPS: {fps:F1}";
                fpsText.color = fpsColor;
            }
            
            if (memoryText != null)
            {
                memoryText.text = $"Memory: {memoryUsage:F1} MB";
            }
            
            if (drawCallText != null)
            {
                Color drawCallColor = drawCalls > 200 ? Color.red : drawCalls > 100 ? Color.yellow : Color.green;
                drawCallText.text = $"Draw Calls: {drawCalls}";
                drawCallText.color = drawCallColor;
            }
            
            if (unitCountText != null)
            {
                unitCountText.text = $"Units: {unitCount} | Buildings: {buildingCount}";
            }
        }
        
        public void ToggleDisplay()
        {
            showMetrics = !showMetrics;
            
            if (fpsText != null)
                fpsText.gameObject.SetActive(showMetrics);
            if (memoryText != null)
                memoryText.gameObject.SetActive(showMetrics);
            if (drawCallText != null)
                drawCallText.gameObject.SetActive(showMetrics);
            if (unitCountText != null)
                unitCountText.gameObject.SetActive(showMetrics);
        }
        
        public void LogPerformanceReport()
        {
            Debug.Log("=== Performance Report ===");
            Debug.Log($"FPS: {fps:F1}");
            Debug.Log($"Memory: {memoryUsage:F1} MB");
            Debug.Log($"Draw Calls: {drawCalls}");
            Debug.Log($"Units: {unitCount}");
            Debug.Log($"Buildings: {buildingCount}");
            Debug.Log("==========================");
        }
        
        private void OnGUI()
        {
            if (showMetrics)
            {
                GUILayout.BeginArea(new Rect(10, 10, 200, 100));
                GUILayout.Label($"FPS: {fps:F1}");
                GUILayout.Label($"Memory: {memoryUsage:F1} MB");
                GUILayout.Label($"Draw Calls: {drawCalls}");
                GUILayout.Label($"Units: {unitCount}");
                GUILayout.EndArea();
            }
        }
    }
}
