using UnityEngine;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Lore
{
    /// <summary>
    /// Central repository of Dune terminology for consistent usage
    /// References: Frank Herbert's Dune series
    /// </summary>
    public static class DuneTerminology
    {
        // Factions
        public static readonly string Atreides = "House Atreides";
        public static readonly string Harkonnen = "House Harkonnen";
        public static readonly string Fremen = "Fremen";
        public static readonly string Sardaukar = "Sardaukar";
        public static readonly string BeneGesserit = "Bene Gesserit";
        public static readonly string SpacingGuild = "Spacing Guild";
        public static readonly string CHOAM = "CHOAM";
        public static readonly string Landsraad = "Landsraad";
        
        // Fremen Terms
        public static readonly string ShaiHulud = "Shai-Hulud";
        public static readonly string MuadDib = "Muad'Dib";
        public static readonly string LisanAlGaib = "Lisan al-Gaib";
        public static readonly string Mahdi = "Mahdi";
        public static readonly string Sietch = "Sietch";
        public static readonly string Naib = "Naib";
        public static readonly string Fedaykin = "Fedaykin";
        public static readonly string Crysknife = "Crysknife";
        public static readonly string Stillsuit = "Stillsuit";
        public static readonly string Maker = "Maker";
        public static readonly string WaterOfLife = "Water of Life";
        
        // Technology
        public static readonly string Shield = "Holtzman Shield";
        public static readonly string Lasgun = "Lasgun";
        public static readonly string Ornithopter = "Ornithopter";
        public static readonly string Carryall = "Carryall";
        public static readonly string Thumper = "Thumper";
        public static readonly string Windtrap = "Windtrap";
        public static readonly string Deathstill = "Deathstill";
        
        // Resources
        public static readonly string Spice = "Spice Melange";
        public static readonly string Water = "Water";
        public static readonly string Sandtrout = "Sandtrout";
        
        // Characters
        public static readonly string PaulAtreides = "Paul Atreides";
        public static readonly string DukeLeto = "Duke Leto Atreides";
        public static readonly string LadyJessica = "Lady Jessica";
        public static readonly string Stilgar = "Stilgar";
        public static readonly string Chani = "Chani";
        public static readonly string BaronHarkonnen = "Baron Vladimir Harkonnen";
        public static readonly string FeydRautha = "Feyd-Rautha Harkonnen";
        public static readonly string GurneyHalleck = "Gurney Halleck";
        public static readonly string DuncanIdaho = "Duncan Idaho";
        public static readonly string ThufirHawat = "Thufir Hawat";
        
        // Locations
        public static readonly string Arrakis = "Arrakis";
        public static readonly string Dune = "Dune";
        public static readonly string Arrakeen = "Arrakeen";
        public static readonly string Carthag = "Carthag";
        public static readonly string SietchTabr = "Sietch Tabr";
        public static readonly string SietchTabr = "Sietch Tabr";
        
        // Titles
        public static readonly string KwisatzHaderach = "Kwisatz Haderach";
        public static readonly string PadishahEmperor = "Padishah Emperor";
        public static readonly string ReverendMother = "Reverend Mother";
        
        // Quotes
        public static readonly string QuoteFear = "Fear is the mind-killer.";
        public static readonly string QuoteSpiceMustFlow = "The spice must flow.";
        public static readonly string QuoteWalkWithoutRhythm = "Walk without rhythm, and you won't attract the worm.";
        public static readonly string QuoteBlessMaker = "Bless the Maker and His water.";
        public static readonly string QuoteSleeperAwakens = "The sleeper must awaken.";
        
        public static string GetFactionDisplayName(GameManager.Faction faction)
        {
            switch (faction)
            {
                case GameManager.Faction.Atreides: return Atreides;
                case GameManager.Faction.Harkonnen: return Harkonnen;
                case GameManager.Faction.Fremen: return Fremen;
                default: return "Unknown";
            }
        }
        
        public static string GetTerminology(string key)
        {
            var fields = typeof(DuneTerminology).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.Name.ToLower() == key.ToLower())
                {
                    return field.GetValue(null) as string;
                }
            }
            return key;
        }
        
        public static string GetLoreQuote(int index)
        {
            string[] quotes = new string[]
            {
                QuoteFear,
                QuoteSpiceMustFlow,
                QuoteWalkWithoutRhythm,
                QuoteBlessMaker,
                QuoteSleeperAwakens
            };
            
            return quotes[index % quotes.Length];
        }
    }
    
    /// <summary>
    /// Dune lore fact database for loading screen and tooltips
    /// </summary>
    [CreateAssetMenu(fileName = "DuneLore", menuName = "Dune/Lore Database")]
    public class DuneLore : ScriptableObject
    {
        [System.Serializable]
        public class LoreEntry
        {
            public string title;
            [TextArea(3, 10)]
            public string fact;
            public string bookReference;
            public Sprite illustration;
        }
        
        public List<LoreEntry> entries;
        
        private static DuneLore _instance;
        public static DuneLore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<DuneLore>("DuneLore");
                }
                return _instance;
            }
        }
        
        public LoreEntry GetRandomLore()
        {
            if (entries == null || entries.Count == 0) return null;
            return entries[Random.Range(0, entries.Count)];
        }
        
        public LoreEntry GetLoreByTitle(string title)
        {
            return entries.Find(e => e.title == title);
        }
    }
}
