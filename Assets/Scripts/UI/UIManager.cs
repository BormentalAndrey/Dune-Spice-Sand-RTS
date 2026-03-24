using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Main UI Manager - Mobile-optimized
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Panels")]
        public GameObject mainMenuPanel;
        public GameObject gameHUDPanel;
        public GameObject pausePanel;
        public GameObject buildingMenuPanel;
        public GameObject unitSelectionPanel;
        
        [Header("Resource Display")]
        public TextMeshProUGUI spiceText;
        public TextMeshProUGUI waterText;
        public Slider jihadSlider;
        
        [Header("Selection")]
        public GameObject selectedUnitPanel;
        public TextMeshProUGUI selectedUnitName;
        public Image selectedUnitIcon;
        
        [Header("Touch")]
        public float dragThreshold = 20f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            ShowMainMenu();
            
            // Subscribe to game events
            GameManager.Instance.OnResourcesUpdated += UpdateResourcesUI;
            GameManager.Instance.OnJihadMeterChanged += UpdateJihadUI;
        }
        
        public void ShowMainMenu()
        {
            mainMenuPanel.SetActive(true);
            gameHUDPanel.SetActive(false);
        }
        
        public void StartCampaign()
        {
            mainMenuPanel.SetActive(false);
            gameHUDPanel.SetActive(true);
            CampaignManager.Instance.StartMission(1);
        }
        
        public void StartSkirmish()
        {
            mainMenuPanel.SetActive(false);
            gameHUDPanel.SetActive(true);
            GameManager.Instance.LoadSkirmish();
        }
        
        private void UpdateResourcesUI(float spice, float water)
        {
            if (spiceText != null)
                spiceText.text = $"🌶️ {spice:F0}";
            if (waterText != null)
                waterText.text = $"💧 {water:F0}";
        }
        
        private void UpdateJihadUI(float value)
        {
            if (jihadSlider != null)
                jihadSlider.value = value / 100f;
        }
        
        public void ShowUnitSelection(UnitBase unit)
        {
            selectedUnitPanel.SetActive(true);
            selectedUnitName.text = unit.unitName;
            // selectedUnitIcon.sprite = unit.icon;
        }
        
        public void HideUnitSelection()
        {
            selectedUnitPanel.SetActive(false);
        }
        
        public void ShowBuildingMenu(Vector3 buildPosition)
        {
            buildingMenuPanel.SetActive(true);
            // TODO: Populate with available buildings
        }
        
        public void ShowMissionComplete(int nextMission)
        {
            // TODO: Show mission complete screen
        }
        
        public void ShowJihadDecision()
        {
            // Show Paul's Jihad decision UI
            // Dune Messiah reference
        }
        
        public void PauseGame()
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
        }
        
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
        }
        
        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
