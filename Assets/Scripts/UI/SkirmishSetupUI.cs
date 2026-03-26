using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Skirmish mode setup screen
    /// </summary>
    public class SkirmishSetupUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;
        
        [Header("Player Settings")]
        public TMP_Dropdown playerFactionDropdown;
        public TMP_InputField playerNameInput;
        
        [Header("AI Settings")]
        public TMP_Dropdown aiFactionDropdown;
        public TMP_Dropdown aiDifficultyDropdown;
        public Toggle enableAI;
        
        [Header("Map Settings")]
        public TMP_Dropdown mapDropdown;
        public Image mapPreview;
        
        [Header("Game Settings")]
        public TMP_Dropdown resourceMultiplierDropdown;
        public Toggle enableWorms;
        public Toggle enableSandstorms;
        public Toggle enableDayNight;
        
        [Header("Buttons")]
        public Button startButton;
        public Button backButton;
        
        private Dictionary<string, string> maps = new Dictionary<string, string>
        {
            { "Desert Plains", "A vast open desert with scattered spice fields" },
            { "Rock Formations", "Rocky terrain with natural defenses" },
            { "Sietch Region", "Fremen territory with hidden sietch entrances" },
            { "Imperial Basin", "Former Harkonnen stronghold with ruins" },
            { "Deep Desert", "Dangerous worm territory with rich spice" }
        };
        
        private void Start()
        {
            if (playerFactionDropdown != null)
            {
                playerFactionDropdown.ClearOptions();
                playerFactionDropdown.AddOptions(new List<string> { "House Atreides", "House Harkonnen", "Fremen" });
                playerFactionDropdown.onValueChanged.AddListener(OnPlayerFactionChanged);
            }
            
            if (aiFactionDropdown != null)
            {
                aiFactionDropdown.ClearOptions();
                aiFactionDropdown.AddOptions(new List<string> { "House Atreides", "House Harkonnen", "Fremen", "Random" });
                aiFactionDropdown.value = 1; // Harkonnen as default enemy
            }
            
            if (aiDifficultyDropdown != null)
            {
                aiDifficultyDropdown.ClearOptions();
                aiDifficultyDropdown.AddOptions(new List<string> { "Easy", "Medium", "Hard", "Insane" });
                aiDifficultyDropdown.value = 1;
            }
            
            if (mapDropdown != null)
            {
                mapDropdown.ClearOptions();
                mapDropdown.AddOptions(new List<string>(maps.Keys));
                mapDropdown.onValueChanged.AddListener(OnMapChanged);
            }
            
            if (resourceMultiplierDropdown != null)
            {
                resourceMultiplierDropdown.ClearOptions();
                resourceMultiplierDropdown.AddOptions(new List<string> { "Low (0.5x)", "Normal (1x)", "High (1.5x)", "Rich (2x)" });
                resourceMultiplierDropdown.value = 1;
            }
            
            if (enableWorms != null) enableWorms.isOn = true;
            if (enableSandstorms != null) enableSandstorms.isOn = true;
            if (enableDayNight != null) enableDayNight.isOn = true;
            if (enableAI != null) enableAI.isOn = true;
            
            if (startButton != null) startButton.onClick.AddListener(StartSkirmish);
            if (backButton != null) backButton.onClick.AddListener(BackToMenu);
            
            // Set default player name
            if (playerNameInput != null)
                playerNameInput.text = SystemInfo.deviceName;
        }
        
        private void OnPlayerFactionChanged(int index)
        {
            GameManager.Faction faction = (GameManager.Faction)index;
            GameManager.Instance.playerFaction = faction;
            
            // Update AI faction based on player choice (suggest opposite)
            if (aiFactionDropdown != null && aiFactionDropdown.value == 3) // Random
            {
                int suggestedAiFaction = index == 0 ? 1 : index == 1 ? 0 : 2;
                aiFactionDropdown.value = suggestedAiFaction;
            }
        }
        
        private void OnMapChanged(int index)
        {
            string mapName = mapDropdown.options[index].text;
            if (mapPreview != null && maps.ContainsKey(mapName))
            {
                // Update map preview image
                // mapPreview.sprite = GetMapPreview(mapName);
            }
        }
        
        private void StartSkirmish()
        {
            // Save skirmish settings
            PlayerPrefs.SetInt("Skirmish_PlayerFaction", playerFactionDropdown.value);
            PlayerPrefs.SetInt("Skirmish_AIFaction", aiFactionDropdown.value);
            PlayerPrefs.SetInt("Skirmish_AIDifficulty", aiDifficultyDropdown.value);
            PlayerPrefs.SetInt("Skirmish_Map", mapDropdown.value);
            PlayerPrefs.SetInt("Skirmish_ResourceMultiplier", resourceMultiplierDropdown.value);
            PlayerPrefs.SetInt("Skirmish_Worms", enableWorms.isOn ? 1 : 0);
            PlayerPrefs.SetInt("Skirmish_Sandstorms", enableSandstorms.isOn ? 1 : 0);
            PlayerPrefs.SetInt("Skirmish_DayNight", enableDayNight.isOn ? 1 : 0);
            PlayerPrefs.Save();
            
            // Load skirmish scene
            LoadingScreenManager.Instance?.LoadScene("Skirmish");
        }
        
        private void BackToMenu()
        {
            panel.SetActive(false);
            UIManager.Instance?.ShowMainMenu();
        }
        
        public void Show()
        {
            panel.SetActive(true);
        }
        
        public void Hide()
        {
            panel.SetActive(false);
        }
        
        public float GetResourceMultiplier()
        {
            switch (resourceMultiplierDropdown.value)
            {
                case 0: return 0.5f;
                case 1: return 1f;
                case 2: return 1.5f;
                case 3: return 2f;
                default: return 1f;
            }
        }
        
        public float GetAIDifficulty()
        {
            switch (aiDifficultyDropdown.value)
            {
                case 0: return 0.5f;
                case 1: return 1f;
                case 2: return 1.5f;
                case 3: return 2f;
                default: return 1f;
            }
        }
        
        public GameManager.Faction GetAIFaction()
        {
            if (aiFactionDropdown.value == 3) // Random
            {
                return (GameManager.Faction)Random.Range(0, 3);
            }
            return (GameManager.Faction)aiFactionDropdown.value;
        }
        
        public string GetMapName()
        {
            return mapDropdown.options[mapDropdown.value].text;
        }
    }
}
