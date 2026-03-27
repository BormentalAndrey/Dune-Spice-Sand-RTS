using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Dune.SpiceAndSand.Data
{
    /// <summary>
    /// Loads and manages JSON data definitions for units and buildings
    /// </summary>
    public class AssetDefinitionsLoader : MonoBehaviour
    {
        public static AssetDefinitionsLoader Instance { get; private set; }
        
        private Dictionary<string, UnitDefinition> unitDefinitions;
        private Dictionary<string, BuildingDefinition> buildingDefinitions;
        
        [System.Serializable]
        public class UnitDefinition
        {
            public string unit_id;
            public string name;
            public string faction;
            public string stats_ref;
            public string prefab_path;
            public string icon_path;
            public string sound_set;
            public string model_path;
            public string material_path;
        }
        
        [System.Serializable]
        public class BuildingDefinition
        {
            public string building_id;
            public string name;
            public string faction;
            public float build_time;
            public GridSize grid_size;
            public string prefab_path;
            public string ghost_prefab_path;
            public string icon_path;
            public string model_path;
            public string material_path;
            public string construction_effect;
        }
        
        [System.Serializable]
        public class GridSize
        {
            public int width;
            public int height;
        }
        
        [System.Serializable]
        private class UnitListWrapper
        {
            public List<UnitDefinition> units;
        }
        
        [System.Serializable]
        private class BuildingListWrapper
        {
            public List<BuildingDefinition> buildings;
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
            
            LoadDefinitions();
        }
        
        private void LoadDefinitions()
        {
            LoadUnitDefinitions();
            LoadBuildingDefinitions();
        }
        
        private void LoadUnitDefinitions()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/UnitDefinitions");
            if (jsonFile != null)
            {
                try
                {
                    UnitListWrapper wrapper = JsonUtility.FromJson<UnitListWrapper>(jsonFile.text);
                    unitDefinitions = new Dictionary<string, UnitDefinition>();
                    
                    foreach (var unit in wrapper.units)
                    {
                        unitDefinitions[unit.unit_id] = unit;
                    }
                    
                    Debug.Log($"Loaded {unitDefinitions.Count} unit definitions");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse unit definitions: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("UnitDefinitions.json not found");
                unitDefinitions = new Dictionary<string, UnitDefinition>();
            }
        }
        
        private void LoadBuildingDefinitions()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/BuildingDefinitions");
            if (jsonFile != null)
            {
                try
                {
                    BuildingListWrapper wrapper = JsonUtility.FromJson<BuildingListWrapper>(jsonFile.text);
                    buildingDefinitions = new Dictionary<string, BuildingDefinition>();
                    
                    foreach (var building in wrapper.buildings)
                    {
                        buildingDefinitions[building.building_id] = building;
                    }
                    
                    Debug.Log($"Loaded {buildingDefinitions.Count} building definitions");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse building definitions: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("BuildingDefinitions.json not found");
                buildingDefinitions = new Dictionary<string, BuildingDefinition>();
            }
        }
        
        public UnitDefinition GetUnitDefinition(string unitId)
        {
            if (unitDefinitions != null && unitDefinitions.ContainsKey(unitId))
                return unitDefinitions[unitId];
            return null;
        }
        
        public BuildingDefinition GetBuildingDefinition(string buildingId)
        {
            if (buildingDefinitions != null && buildingDefinitions.ContainsKey(buildingId))
                return buildingDefinitions[buildingId];
            return null;
        }
        
        public GameObject LoadUnitPrefab(string unitId)
        {
            UnitDefinition def = GetUnitDefinition(unitId);
            if (def != null && !string.IsNullOrEmpty(def.prefab_path))
            {
                return Resources.Load<GameObject>(def.prefab_path.Replace("Assets/", "").Replace(".prefab", ""));
            }
            return null;
        }
        
        public Sprite LoadUnitIcon(string unitId)
        {
            UnitDefinition def = GetUnitDefinition(unitId);
            if (def != null && !string.IsNullOrEmpty(def.icon_path))
            {
                string path = def.icon_path.Replace("Assets/", "").Replace(".png", "");
                return Resources.Load<Sprite>(path);
            }
            return null;
        }
    }
}
