using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Visual
{
    /// <summary>
    /// Fog of war system - Strategic vision and exploration
    /// References: Dune, Book II - Prescience and reconnaissance
    /// </summary>
    public class FogOfWar : MonoBehaviour
    {
        [Header("Fog Settings")]
        public int mapWidth = 200;
        public int mapHeight = 200;
        public float cellSize = 1f;
        public float visionRadius = 15f;
        
        [Header("Visual")]
        public Material fogMaterial;
        public Texture2D fogTexture;
        public Color fogColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        [Header("Reveal Settings")]
        public float revealDuration = 5f;
        public bool permanentReveal = false;
        
        private bool[,] exploredCells;
        private bool[,] visibleCells;
        private Texture2D visibilityTexture;
        private Renderer fogPlaneRenderer;
        
        [Header("Player Units")]
        public List<UnitBase> playerUnits = new List<UnitBase>();
        
        private void Start()
        {
            InitializeFogMap();
            StartCoroutine(UpdateVisibility());
        }
        
        private void InitializeFogMap()
        {
            exploredCells = new bool[mapWidth, mapHeight];
            visibleCells = new bool[mapWidth, mapHeight];
            
            // Create fog plane
            GameObject fogPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            fogPlane.transform.position = new Vector3(mapWidth * cellSize / 2f, 5f, mapHeight * cellSize / 2f);
            fogPlane.transform.localScale = new Vector3(mapWidth * cellSize / 10f, 1, mapHeight * cellSize / 10f);
            fogPlaneRenderer = fogPlane.GetComponent<Renderer>();
            
            // Create visibility texture
            visibilityTexture = new Texture2D(mapWidth, mapHeight);
            visibilityTexture.filterMode = FilterMode.Point;
            fogMaterial.mainTexture = visibilityTexture;
            fogPlaneRenderer.material = fogMaterial;
            
            UpdateFogTexture();
        }
        
        private IEnumerator UpdateVisibility()
        {
            while (true)
            {
                // Reset visible cells
                for (int x = 0; x < mapWidth; x++)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        visibleCells[x, y] = false;
                    }
                }
                
                // Update player units list
                playerUnits.Clear();
                UnitBase[] allUnits = FindObjectsOfType<UnitBase>();
                foreach (var unit in allUnits)
                {
                    if (unit.faction == GameManager.Instance.playerFaction)
                    {
                        playerUnits.Add(unit);
                    }
                }
                
                // Calculate visibility from each unit
                foreach (var unit in playerUnits)
                {
                    Vector2Int unitCell = WorldToCell(unit.transform.position);
                    RevealCircle(unitCell.x, unitCell.y, visionRadius);
                }
                
                // Reveal from buildings
                BuildingBase[] buildings = FindObjectsOfType<BuildingBase>();
                foreach (var building in buildings)
                {
                    if (building.faction == GameManager.Instance.playerFaction)
                    {
                        Vector2Int buildingCell = WorldToCell(building.transform.position);
                        RevealCircle(buildingCell.x, buildingCell.y, visionRadius * 0.8f);
                    }
                }
                
                // Apply Atreides prescience bonus
                if (GameManager.Instance.playerFaction == GameManager.Faction.Atreides)
                {
                    ApplyPrescienceBonus();
                }
                
                UpdateFogTexture();
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        private void RevealCircle(int centerX, int centerY, float radius)
        {
            int radiusInt = Mathf.CeilToInt(radius);
            
            for (int x = -radiusInt; x <= radiusInt; x++)
            {
                for (int y = -radiusInt; y <= radiusInt; y++)
                {
                    int cellX = centerX + x;
                    int cellY = centerY + y;
                    
                    if (cellX >= 0 && cellX < mapWidth && cellY >= 0 && cellY < mapHeight)
                    {
                        float distance = Mathf.Sqrt(x * x + y * y);
                        if (distance <= radius)
                        {
                            visibleCells[cellX, cellY] = true;
                            exploredCells[cellX, cellY] = true;
                        }
                    }
                }
            }
        }
        
        private void ApplyPrescienceBonus()
        {
            // Prescience reveals enemy movements - Dune, Book II
            AtreidesPrescience prescience = FindObjectOfType<AtreidesPrescience>();
            if (prescience != null && prescience.isPrescienceActive)
            {
                // Reveal all enemy units temporarily
                UnitBase[] allUnits = FindObjectsOfType<UnitBase>();
                foreach (var unit in allUnits)
                {
                    if (unit.faction != GameManager.Instance.playerFaction)
                    {
                        Vector2Int unitCell = WorldToCell(unit.transform.position);
                        RevealCircle(unitCell.x, unitCell.y, 5f);
                    }
                }
            }
        }
        
        private void UpdateFogTexture()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Color pixelColor;
                    
                    if (visibleCells[x, y])
                    {
                        // Fully visible - transparent
                        pixelColor = new Color(0, 0, 0, 0);
                    }
                    else if (exploredCells[x, y])
                    {
                        // Explored but not visible - dark grey
                        pixelColor = fogColor;
                        pixelColor.a = 0.5f;
                    }
                    else
                    {
                        // Unexplored - black
                        pixelColor = fogColor;
                        pixelColor.a = 0.95f;
                    }
                    
                    visibilityTexture.SetPixel(x, y, pixelColor);
                }
            }
            
            visibilityTexture.Apply();
        }
        
        public bool IsCellVisible(Vector3 worldPosition)
        {
            Vector2Int cell = WorldToCell(worldPosition);
            if (cell.x >= 0 && cell.x < mapWidth && cell.y >= 0 && cell.y < mapHeight)
            {
                return visibleCells[cell.x, cell.y];
            }
            return false;
        }
        
        public bool IsCellExplored(Vector3 worldPosition)
        {
            Vector2Int cell = WorldToCell(worldPosition);
            if (cell.x >= 0 && cell.x < mapWidth && cell.y >= 0 && cell.y < mapHeight)
            {
                return exploredCells[cell.x, cell.y];
            }
            return false;
        }
        
        private Vector2Int WorldToCell(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt(worldPosition.x / cellSize);
            int y = Mathf.FloorToInt(worldPosition.z / cellSize);
            return new Vector2Int(x, y);
        }
        
        public void RevealArea(Vector3 center, float radius)
        {
            Vector2Int centerCell = WorldToCell(center);
            RevealCircle(centerCell.x, centerCell.y, radius);
        }
        
        public void RevealPermanently(Vector3 position)
        {
            Vector2Int cell = WorldToCell(position);
            exploredCells[cell.x, cell.y] = true;
            visibleCells[cell.x, cell.y] = true;
            UpdateFogTexture();
        }
    }
}
