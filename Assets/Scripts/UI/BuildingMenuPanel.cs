using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Dune.SpiceAndSand.Buildings;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Building construction menu
    /// References: Dune architecture and construction
    /// </summary>
    public class BuildingMenuPanel : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;
        public RectTransform buildingListContainer;
        
        [Header("Building Button Prefab")]
        public GameObject buildingButtonPrefab;
        
        [Header("Building Info")]
        public TextMeshProUGUI buildingNameText;
        public TextMeshProUGUI buildingDescText;
        public TextMeshProUGUI buildingCostText;
        public Image buildingIcon;
        
        [Header("Resources")]
        public TextMeshProUGUI spiceText;
        public TextMeshProUGUI waterText;
        
        [Header("Current Building")]
        public BuildingBase currentBuilding;
        
        private Dictionary<GameManager.Faction, List<BuildingData>> availableBuildings = 
            new Dictionary<GameManager.Faction, List<BuildingData>>();
        private Vector3 buildPosition;
        private bool isPlacementMode = false;
        
        private void Start()
        {
            InitializeBuildings();
            UpdateResourceDisplay();
            
            if (panel != null) panel.SetActive(false);
        }
        
        private void InitializeBuildings()
        {
            // Atreides buildings
            availableBuildings[GameManager.Faction.Atreides] = new List<BuildingData>
            {
                BuildingData.CommandCenter(),
                BuildingData.Barracks(),
                BuildingData.SpiceRefinery(),
                BuildingData.Windtrap(),
                BuildingData.OrnithopterBay(),
                BuildingData.ShieldGenerator()
            };
            
            // Harkonnen buildings
            availableBuildings[GameManager.Faction.Harkonnen] = new List<BuildingData>
            {
                BuildingData.CommandCenter(),
                BuildingData.Barracks(),
                BuildingData.SpiceRefinery(),
                BuildingData.Windtrap(),
                BuildingData.HarkonnenFactory(),
                BuildingData.ShieldGenerator()
            };
            
            // Fremen buildings
            availableBuildings[GameManager.Faction.Fremen] = new List<BuildingData>
            {
                BuildingData.SietchEntrance(),
                BuildingData.FremenBarracks(),
                BuildingData.SpiceRefinery(),
                BuildingData.Windtrap(),
                BuildingData.Deathstill(),
                BuildingData.TerraformingStation()
            };
        }
        
        public void ShowMenu(Vector3 position)
        {
            buildPosition = position;
            isPlacementMode = true;
            
            // Populate building list
            PopulateBuildingList();
            
            // Update resource display
            UpdateResourceDisplay();
            
            // Show panel
            if (panel != null) panel.SetActive(true);
        }
        
        private void PopulateBuildingList()
        {
            // Clear existing buttons
            foreach (Transform child in buildingListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Get buildings for current faction
            GameManager.Faction faction = GameManager.Instance.playerFaction;
            if (!availableBuildings.ContainsKey(faction)) return;
            
            foreach (var buildingData in availableBuildings[faction])
            {
                GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingListContainer);
                BuildingButton button = buttonObj.GetComponent<BuildingButton>();
                
                if (button != null)
                {
                    button.Initialize(buildingData, this);
                }
            }
        }
        
        public void SelectBuilding(BuildingData buildingData)
        {
            if (buildingData == null) return;
            
            // Check if can afford
            bool canAffordSpice = GameManager.Instance.spice >= buildingData.spiceCost;
            bool canAffordWater = GameManager.Instance.water >= buildingData.waterCost;
            
            if (!canAffordSpice || !canAffordWater)
            {
                NotificationSystem.Instance?.ShowMessage(
                    $"Cannot afford {buildingData.buildingName}. Need {buildingData.spiceCost} spice and {buildingData.waterCost} water.",
                    NotificationSystem.NotificationType.Warning);
                return;
            }
            
            // Show placement preview
            StartCoroutine(PlaceBuilding(buildingData));
        }
        
        private System.Collections.IEnumerator PlaceBuilding(BuildingData buildingData)
        {
            // Show placement preview
            GameObject preview = CreatePreview(buildingData);
            
            bool placed = false;
            while (!placed && isPlacementMode)
            {
                // Update preview position with touch
                Vector3 currentPos = GetTouchPosition();
                if (preview != null) preview.transform.position = currentPos;
                
                // Check placement validity
                bool isValid = IsValidPlacement(currentPos);
                UpdatePreviewColor(preview, isValid);
                
                // Check for placement confirm
                if (Input.GetMouseButtonDown(0) && isValid)
                {
                    BuildBuilding(buildingData, currentPos);
                    placed = true;
                }
                
                // Cancel placement
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    placed = true;
                }
                
                yield return null;
            }
            
            if (preview != null) Destroy(preview);
            HideMenu();
        }
        
        private GameObject CreatePreview(BuildingData buildingData)
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            preview.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.5f);
            preview.transform.localScale = new Vector3(5f, 1f, 5f);
            return preview;
        }
        
        private Vector3 GetTouchPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
            
            return buildPosition;
        }
        
        private bool IsValidPlacement(Vector3 position)
        {
            // Check if position is on sand/rock (not water)
            // Check if not overlapping other buildings
            // Check if within map bounds
            
            Collider[] colliders = Physics.OverlapSphere(position, 3f);
            foreach (var collider in colliders)
            {
                if (collider.GetComponent<BuildingBase>() != null)
                {
                    return false; // Overlaps existing building
                }
            }
            
            return true;
        }
        
        private void UpdatePreviewColor(GameObject preview, bool isValid)
        {
            if (preview == null) return;
            
            Renderer renderer = preview.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isValid ? 
                    new Color(0, 1, 0, 0.5f) : 
                    new Color(1, 0, 0, 0.5f);
            }
        }
        
        private void BuildBuilding(BuildingData buildingData, Vector3 position)
        {
            // Deduct resources
            GameManager.Instance.ConsumeSpice(buildingData.spiceCost);
            GameManager.Instance.ConsumeWater(buildingData.waterCost);
            
            // Instantiate building
            GameObject buildingPrefab = GetBuildingPrefab(buildingData);
            if (buildingPrefab != null)
            {
                GameObject buildingObj = Instantiate(buildingPrefab, position, Quaternion.identity);
                BuildingBase building = buildingObj.GetComponent<BuildingBase>();
                
                if (building != null)
                {
                    building.buildingData = buildingData;
                    building.faction = GameManager.Instance.playerFaction;
                }
            }
            
            // Record analytics
            AnalyticsManager.Instance?.TrackBuildingEvent(buildingData.buildingName, "built");
            
            // Show notification
            NotificationSystem.Instance?.ShowBuildingComplete(buildingData.buildingName);
        }
        
        private GameObject GetBuildingPrefab(BuildingData buildingData)
        {
            // Return appropriate prefab based on building type
            // In production, use PrefabReferences
            return null;
        }
        
        private void UpdateResourceDisplay()
        {
            if (spiceText != null)
                spiceText.text = $"🌶️ {GameManager.Instance.spice:F0}";
            if (waterText != null)
                waterText.text = $"💧 {GameManager.Instance.water:F0}";
        }
        
        public void HideMenu()
        {
            isPlacementMode = false;
            if (panel != null) panel.SetActive(false);
        }
        
        private void Update()
        {
            if (isPlacementMode && Input.GetKeyDown(KeyCode.Escape))
            {
                HideMenu();
            }
        }
    }
    
    /// <summary>
    /// Individual building button in menu
    /// </summary>
    public class BuildingButton : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI costText;
        public Image icon;
        public Button button;
        
        private BuildingData buildingData;
        private BuildingMenuPanel menuPanel;
        
        public void Initialize(BuildingData data, BuildingMenuPanel panel)
        {
            buildingData = data;
            menuPanel = panel;
            
            if (nameText != null) nameText.text = data.buildingName;
            if (costText != null) costText.text = $"🌶️{data.spiceCost} 💧{data.waterCost}";
            
            if (button != null)
                button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            menuPanel?.SelectBuilding(buildingData);
        }
        
        private void Update()
        {
            // Update button interactability based on resources
            if (button != null && buildingData != null)
            {
                bool canAfford = GameManager.Instance.spice >= buildingData.spiceCost &&
                                 GameManager.Instance.water >= buildingData.waterCost;
                button.interactable = canAfford;
            }
        }
    }
    
    // Additional building data for factions
    public static class BuildingDataExtensions
    {
        public static BuildingData HarkonnenFactory()
        {
            BuildingData data = BuildingData.CommandCenter();
            data.buildingName = "Harkonnen Factory";
            data.maxHealth = 800f;
            data.spiceCost = 400f;
            data.waterCost = 80f;
            data.bookReference = "Dune, Book I, Chapter 10";
            return data;
        }
        
        public static BuildingData FremenBarracks()
        {
            BuildingData data = BuildingData.Barracks();
            data.buildingName = "Fremen Training Ground";
            data.spiceCost = 150f;
            data.waterCost = 0f;
            data.bookReference = "Dune, Book I, Chapter 25";
            return data;
        }
        
        public static BuildingData TerraformingStation()
        {
            BuildingData data = CreateInstance<BuildingData>();
            data.buildingName = "Terraforming Station";
            data.maxHealth = 1000f;
            data.spiceCost = 1000f;
            data.waterCost = 500f;
            data.constructionTime = 20f;
            data.bookReference = "Dune Messiah, Chapter 5";
            return data;
        }
        
        public static BuildingData Barracks()
        {
            BuildingData data = CreateInstance<BuildingData>();
            data.buildingName = "Barracks";
            data.maxHealth = 400f;
            data.armor = 15f;
            data.spiceCost = 200f;
            data.waterCost = 50f;
            data.constructionTime = 6f;
            data.bookReference = "Dune, Book I, Chapter 8";
            return data;
        }
    }
}
