using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dune.SpiceAndSand.Localization
{
    /// <summary>
    /// Localization manager for multi-language support
    /// References: Dune terminology in multiple languages
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        
        [Header("Settings")]
        public SystemLanguage defaultLanguage = SystemLanguage.English;
        public bool autoDetectLanguage = true;
        
        [Header("Current")]
        public SystemLanguage currentLanguage;
        
        private Dictionary<string, string> localizedText = new Dictionary<string, string>();
        private Dictionary<SystemLanguage, string> languageFiles = new Dictionary<SystemLanguage, string>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeLanguages();
            LoadLanguage(GetSystemLanguage());
        }
        
        private void InitializeLanguages()
        {
            languageFiles[SystemLanguage.English] = "en";
            languageFiles[SystemLanguage.French] = "fr";
            languageFiles[SystemLanguage.German] = "de";
            languageFiles[SystemLanguage.Spanish] = "es";
            languageFiles[SystemLanguage.Italian] = "it";
            languageFiles[SystemLanguage.Japanese] = "ja";
            languageFiles[SystemLanguage.ChineseSimplified] = "zh-CN";
            languageFiles[SystemLanguage.Russian] = "ru";
            languageFiles[SystemLanguage.Portuguese] = "pt";
            languageFiles[SystemLanguage.Arabic] = "ar";
        }
        
        private SystemLanguage GetSystemLanguage()
        {
            if (autoDetectLanguage)
            {
                SystemLanguage systemLang = Application.systemLanguage;
                if (languageFiles.ContainsKey(systemLang))
                {
                    return systemLang;
                }
            }
            
            // Load saved language preference
            string savedLang = PlayerPrefs.GetString("Language", "");
            if (!string.IsNullOrEmpty(savedLang))
            {
                foreach (var lang in languageFiles)
                {
                    if (lang.Value == savedLang)
                    {
                        return lang.Key;
                    }
                }
            }
            
            return defaultLanguage;
        }
        
        public void LoadLanguage(SystemLanguage language)
        {
            if (!languageFiles.ContainsKey(language))
            {
                language = defaultLanguage;
            }
            
            currentLanguage = language;
            string languageCode = languageFiles[language];
            
            // Load localization file
            TextAsset textAsset = Resources.Load<TextAsset>($"Localization/{languageCode}");
            if (textAsset != null)
            {
                LoadLocalizationData(textAsset.text);
                PlayerPrefs.SetString("Language", languageCode);
                PlayerPrefs.Save();
                
                // Refresh UI
                OnLanguageChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"Localization file not found for {language}");
            }
        }
        
        private void LoadLocalizationData(string jsonData)
        {
            localizedText.Clear();
            
            try
            {
                LocalizationData data = JsonUtility.FromJson<LocalizationData>(jsonData);
                foreach (var entry in data.entries)
                {
                    localizedText[entry.key] = entry.value;
                }
                
                Debug.Log($"Loaded {localizedText.Count} localization entries");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse localization data: {e.Message}");
            }
        }
        
        public string GetText(string key)
        {
            if (localizedText.ContainsKey(key))
            {
                return localizedText[key];
            }
            
            Debug.LogWarning($"Missing localization key: {key}");
            return key;
        }
        
        public string GetText(string key, params object[] args)
        {
            string text = GetText(key);
            return string.Format(text, args);
        }
        
        public SystemLanguage[] GetAvailableLanguages()
        {
            List<SystemLanguage> available = new List<SystemLanguage>();
            foreach (var lang in languageFiles.Keys)
            {
                string path = $"Localization/{languageFiles[lang]}";
                if (Resources.Load<TextAsset>(path) != null)
                {
                    available.Add(lang);
                }
            }
            return available.ToArray();
        }
        
        public event System.Action OnLanguageChanged;
        
        [System.Serializable]
        public class LocalizationData
        {
            public LocalizationEntry[] entries;
        }
        
        [System.Serializable]
        public class LocalizationEntry
        {
            public string key;
            public string value;
        }
        
        // Dune-specific localization keys
        public static class Keys
        {
            // UI
            public const string Play = "ui.play";
            public const string Settings = "ui.settings";
            public const string Quit = "ui.quit";
            public const string Resume = "ui.resume";
            public const string Pause = "ui.pause";
            
            // Resources
            public const string Spice = "resource.spice";
            public const string Water = "resource.water";
            public const string SpiceMustFlow = "resource.spice_must_flow";
            
            // Units
            public const string AtreidesTrooper = "unit.atreides_trooper";
            public const string FremenFedaykin = "unit.fremen_fedaykin";
            public const string Sandworm = "unit.sandworm";
            
            // Buildings
            public const string Windtrap = "building.windtrap";
            public const string SpiceRefinery = "building.spice_refinery";
            public const string Sietch = "building.sietch";
            
            // Mechanics
            public const string ShieldActive = "mechanic.shield_active";
            public const string WormApproaching = "mechanic.worm_approaching";
            public const string WalkWithoutRhythm = "mechanic.walk_without_rhythm";
            
            // Campaign
            public const string Mission1Title = "campaign.mission1.title";
            public const string Mission1Desc = "campaign.mission1.description";
        }
    }
}
