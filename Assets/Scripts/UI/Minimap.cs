using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;
using Dune.SpiceAndSand.Buildings;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Minimap system for strategic overview
    /// References: Dune tactical maps and reconnaissance
    /// </summary>
    public class Minimap : MonoBehaviour
    {
        [Header("Minimap Settings")]
        public RawImage minimapImage;
        public RectTransform minimapRect;
        public Camera minimapCamera;
        public int minimapResolution = 256;
        
        [Header("Icons")]
        public GameObject unitIconPrefab;
        public GameObject buildingIconPrefab;
        public GameObject spiceFieldIconPrefab;
        public GameObject wormIconPrefab;
        
        [Header("Colors")]
        public Color atreidesColor = Color.blue;
        public Color harkonnenColor = Color.red;
        public Color fremenColor = Color.green;
        public Color neutralColor = Color.gray;
        public Color spiceColor = new Color(1f, 0.7f, 0.2f);
        public Color wormColor = new Color(0.5f, 0.2f, 0f);
        
        [Header("Interaction")]
        public bool allowClickToMove = true;
        public bool showFogOfWar = true;
        
        private RectTransform iconContainer;
        private Dictionary<int, RectTransform> unitIcons = new Dictionary<int, RectTransform>();
        private Dictionary<int, RectTransform> buildingIcons = new Dictionary<int, RectTransform>();
        private List<RectTransform> spiceIcons = new List<RectTransform>();
        
        private Terrain terrain;
        private Bounds worldBounds;
        private Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
            terrain = FindObjectOfType<Terrain>();
            
            if (terrain != null)
            {
                worldBounds = new Bounds(
                    new Vector3(terrain.terrainData.size.x / 2f, 0, terrain.terrainData.size.z / 2f),
                    terrain.terrainData.size
                );
            }
            
            // Create icon container
            GameObject container = new GameObject("IconContainer");
            iconContainer = container.AddComponent<RectTransform>();
            iconContainer.SetParent(minimapRect);
            iconContainer.anchorMin = Vector2.zero;
            iconContainer.anchorMax = Vector2.one;
            iconContainer.offsetMin = Vector2.zero;
            iconContainer.offsetMax = Vector2.zero;
            
            // Setup minimap camera
            if (minimapCamera != null)
            {
                minimapCamera.orthographic = true;
                minimapCamera.orthographicSize = worldBounds.extents.z;
                minimapCamera.transform.position = new Vector3(worldBounds.center.x, 100f, worldBounds.center.z);
                minimapCamera.transform.rotation = Quaternion.Euler(90f, 0, 0);
            }
            
            StartCoroutine(UpdateMinimap());
            StartCoroutine(UpdateIcons());
        }
        
        private System.Collections.IEnumerator UpdateMinimap()
        {
            while (true)
            {
                // Render minimap texture
                if (minimapCamera != null && minimapImage != null)
                {
                    RenderTexture rt = new RenderTexture(minimapResolution, minimapResolution, 16);
                    minimapCamera.targetTexture = rt;
                    minimapCamera.Render();
                    minimapImage.texture = rt;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private System.Collections.IEnumerator UpdateIcons()
        {
            while (true)
            {
                // Update unit icons
                UpdateUnitIcons();
                
                // Update building icons
                UpdateBuildingIcons();
                
                // Update spice field icons
                UpdateSpiceIcons();
                
                // Update worm icons
                UpdateWormIcons();
                
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        private void UpdateUnitIcons()
        {
            UnitBase[] units = FindObjectsOfType<UnitBase>();
            HashSet<int> activeUnits = new HashSet<int>();
            
            foreach (var unit in units)
            {
                int instanceId = unit.GetInstanceID();
                activeUnits.Add(instanceId);
                
                if (!unitIcons.ContainsKey(instanceId))
                {
                    // Create new icon
                    GameObject iconObj = Instantiate(unitIconPrefab, iconContainer);
                    RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                    unitIcons[instanceId] = iconRect;
                    
                    // Set color based on faction
                    Image iconImage = iconObj.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        switch (unit.faction)
                        {
                            case UnitBase.Faction.Atreides:
                                iconImage.color = atreidesColor;
                                break;
                            case UnitBase.Faction.Harkonnen:
                                iconImage.color = harkonnenColor;
                                break;
                            case UnitBase.Faction.Fremen:
                                iconImage.color = fremenColor;
                                break;
                            default:
                                iconImage.color = neutralColor;
                                break;
                        }
                    }
                }
                
                // Update position
                Vector2 minimapPos = WorldToMinimap(unit.transform.position);
                unitIcons[instanceId].anchoredPosition = minimapPos;
            }
            
            // Remove dead units
            List<int> toRemove = new List<int>();
            foreach (var kvp in unitIcons)
            {
                if (!activeUnits.Contains(kvp.Key))
                {
                    Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (int id in toRemove)
            {
                unitIcons.Remove(id);
            }
        }
        
        private void UpdateBuildingIcons()
        {
            BuildingBase[] buildings = FindObjectsOfType<BuildingBase>();
            HashSet<int> activeBuildings = new HashSet<int>();
            
            foreach (var building in buildings)
            {
                int instanceId = building.GetInstanceID();
                activeBuildings.Add(instanceId);
                
                if (!buildingIcons.ContainsKey(instanceId))
                {
                    GameObject iconObj = Instantiate(buildingIconPrefab, iconContainer);
                    RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                    buildingIcons[instanceId] = iconRect;
                }
                
                Vector2 minimapPos = WorldToMinimap(building.transform.position);
                buildingIcons[instanceId].anchoredPosition = minimapPos;
            }
            
            // Remove destroyed buildings
            List<int> toRemove = new List<int>();
            foreach (var kvp in buildingIcons)
            {
                if (!activeBuildings.Contains(kvp.Key))
                {
                    Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (int id in toRemove)
            {
                buildingIcons.Remove(id);
            }
        }
        
        private void UpdateSpiceIcons()
        {
            SpiceField[] spiceFields = FindObjectsOfType<SpiceField>();
            
            // Clear existing icons if count mismatch
            if (spiceIcons.Count != spiceFields.Length)
            {
                foreach (var icon in spiceIcons)
                {
                    Destroy(icon.gameObject);
                }
                spiceIcons.Clear();
                
                foreach (var field in spiceFields)
                {
                    GameObject iconObj = Instantiate(spiceFieldIconPrefab, iconContainer);
                    RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                    spiceIcons.Add(iconRect);
                    
                    Image iconImage = iconObj.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.color = spiceColor;
                    }
                }
            }
            
            // Update positions
            for (int i = 0; i < spiceFields.Length && i < spiceIcons.Count; i++)
            {
                Vector2 minimapPos = WorldToMinimap(spiceFields[i].transform.position);
                spiceIcons[i].anchoredPosition = minimapPos;
                
                // Fade if depleted
                Image iconImage = spiceIcons[i].GetComponent<Image>();
                if (iconImage != null)
                {
                    float alpha = spiceFields[i].HasSpice ? 1f : 0.3f;
                    Color color = iconImage.color;
                    color.a = alpha;
                    iconImage.color = color;
                }
            }
        }
        
        private void UpdateWormIcons()
        {
            SandwormAI[] worms = FindObjectsOfType<SandwormAI>();
            
            // Update worm icons (simplified for now)
            // In production, add worm icons similar to units
        }
        
        private Vector2 WorldToMinimap(Vector3 worldPos)
        {
            // Convert world position to minimap coordinates
            float x = (worldPos.x - worldBounds.min.x) / worldBounds.size.x;
            float z = (worldPos.z - worldBounds.min.z) / worldBounds.size.z;
            
            return new Vector2(x * minimapRect.rect.width, z * minimapRect.rect.height);
        }
        
        private Vector3 MinimapToWorld(Vector2 minimapPos)
        {
            // Convert minimap coordinates to world position
            float x = worldBounds.min.x + (minimapPos.x / minimapRect.rect.width) * worldBounds.size.x;
            float z = worldBounds.min.z + (minimapPos.y / minimapRect.rect.height) * worldBounds.size.z;
            
            // Get terrain height
            float y = 0;
            if (terrain != null)
            {
                y = terrain.SampleHeight(new Vector3(x, 0, z));
            }
            
            return new Vector3(x, y, z);
        }
        
        private void Update()
        {
            // Handle minimap clicks
            if (allowClickToMove && Input.GetMouseButtonDown(0))
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    minimapRect,
                    Input.mousePosition,
                    null,
                    out localPoint
                );
                
                if (minimapRect.rect.Contains(localPoint))
                {
                    Vector3 worldPos = MinimapToWorld(localPoint);
                    CameraController.Instance?.FocusOnPosition(worldPos);
                    
                    // Move selected units
                    TouchInputSystem inputSystem = FindObjectOfType<TouchInputSystem>();
                    if (inputSystem != null)
                    {
                        // inputSystem.MoveSelectedUnits(worldPos);
                    }
                }
            }
        }
        
        public void ToggleMinimap()
        {
            minimapImage.gameObject.SetActive(!minimapImage.gameObject.activeSelf);
        }
    }
}
