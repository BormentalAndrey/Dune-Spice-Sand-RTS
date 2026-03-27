using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Dune.SpiceAndSand.Lore
{
    /// <summary>
    /// Manages Dune lore database for loading screens, tooltips, and flavor text
    /// Reference: Frank Herbert's Dune series
    /// </summary>
    public class LoreManager : MonoBehaviour
    {
        public static LoreManager Instance { get; private set; }
        
        [Header("Database")]
        private DuneLoreDatabase loreDatabase;
        private bool isLoaded = false;
        
        [Header("Cached Collections")]
        private List<LoreQuote> loadingQuotes;
        private Dictionary<string, FactionLore> factionLore;
        private Dictionary<string, MechanicLore> mechanicLore;
        private Dictionary<string, UnitLore> unitLore;
        private Dictionary<string, BuildingLore> buildingLore;
        
        [System.Serializable]
        public class DuneLoreDatabase
        {
            public string version;
            public string last_updated;
            public List<LoreQuote> loading_quotes;
            public List<FactionLore> factions_lore;
            public List<MechanicLore> mechanics_lore;
            public List<UnitLore> units_lore;
            public List<BuildingLore> buildings_lore;
        }
        
        [System.Serializable]
        public class LoreQuote
        {
            public string id;
            public string text;
            public string source;
            public string speaker;
            public string category;
        }
        
        [System.Serializable]
        public class FactionLore
        {
            public string faction;
            public string description;
            public string quote;
            public string source;
            public List<string> strengths;
            public List<string> weaknesses;
        }
        
        [System.Serializable]
        public class MechanicLore
        {
            public string mechanic;
            public string name;
            public string description;
            public string quote;
            public string source;
            public string game_rule;
        }
        
        [System.Serializable]
        public class UnitLore
        {
            public string unit_id;
            public string lore;
            public string quote;
            public string source;
        }
        
        [System.Serializable]
        public class BuildingLore
        {
            public string building_id;
            public string lore;
            public string quote;
            public string source;
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
            
            LoadLoreDatabase();
        }
        
        private void LoadLoreDatabase()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("Data/DuneLoreDatabase");
            if (jsonFile != null)
            {
                try
                {
                    loreDatabase = JsonUtility.FromJson<DuneLoreDatabase>(jsonFile.text);
                    isLoaded = true;
                    
                    // Cache collections
                    loadingQuotes = loreDatabase.loading_quotes;
                    
                    factionLore = loreDatabase.factions_lore.ToDictionary(f => f.faction, f => f);
                    mechanicLore = loreDatabase.mechanics_lore.ToDictionary(m => m.mechanic, m => m);
                    unitLore = loreDatabase.units_lore.ToDictionary(u => u.unit_id, u => u);
                    buildingLore = loreDatabase.buildings_lore.ToDictionary(b => b.building_id, b => b);
                    
                    Debug.Log($"Lore database loaded: {loadingQuotes.Count} quotes, " +
                              $"{factionLore.Count} factions, {mechanicLore.Count} mechanics");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse lore database: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("DuneLoreDatabase.json not found in Resources/Data/");
                InitializeFallbackLore();
            }
        }
        
        private void InitializeFallbackLore()
        {
            loadingQuotes = new List<LoreQuote>
            {
                new LoreQuote { text = "Fear is the mind-killer.", source = "Dune, Book I", speaker = "Paul Atreides" },
                new LoreQuote { text = "The spice must flow.", source = "Dune, Book I", speaker = "Duke Leto" }
            };
            Debug.Log("Using fallback lore database");
        }
        
        public string GetRandomLoadingQuote()
        {
            if (!isLoaded || loadingQuotes == null || loadingQuotes.Count == 0)
                return "The spice must flow.";
                
            int index = Random.Range(0, loadingQuotes.Count);
            LoreQuote quote = loadingQuotes[index];
            
            return $"\"{quote.text}\"\n— {quote.speaker}, {quote.source}";
        }
        
        public string GetQuoteByCategory(string category)
        {
            var quotes = loadingQuotes?.Where(q => q.category == category).ToList();
            if (quotes == null || quotes.Count == 0)
                return GetRandomLoadingQuote();
                
            int index = Random.Range(0, quotes.Count);
            return $"\"{quotes[index].text}\"\n— {quotes[index].speaker}";
        }
        
        public string GetFactionDescription(string factionName)
        {
            if (factionLore != null && factionLore.ContainsKey(factionName))
                return factionLore[factionName].description;
            return "A faction of the Imperium.";
        }
        
        public string GetFactionQuote(string factionName)
        {
            if (factionLore != null && factionLore.ContainsKey(factionName))
                return factionLore[factionName].quote;
            return "";
        }
        
        public string GetMechanicDescription(string mechanicName)
        {
            if (mechanicLore != null && mechanicLore.ContainsKey(mechanicName))
                return mechanicLore[mechanicName].description;
            return "";
        }
        
        public string GetMechanicGameRule(string mechanicName)
        {
            if (mechanicLore != null && mechanicLore.ContainsKey(mechanicName))
                return mechanicLore[mechanicName].game_rule;
            return "";
        }
        
        public string GetUnitLore(string unitId)
        {
            if (unitLore != null && unitLore.ContainsKey(unitId))
                return unitLore[unitId].lore;
            return "";
        }
        
        public string GetUnitQuote(string unitId)
        {
            if (unitLore != null && unitLore.ContainsKey(unitId))
                return unitLore[unitId].quote;
            return "";
        }
        
        public string GetBuildingLore(string buildingId)
        {
            if (buildingLore != null && buildingLore.ContainsKey(buildingId))
                return buildingLore[buildingId].lore;
            return "";
        }
        
        public bool IsLoaded()
        {
            return isLoaded;
        }
    }
}
