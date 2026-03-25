using UnityEngine;
using System.Collections;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.UI;
using Dune.SpiceAndSand.Audio;
using Dune.SpiceAndSand.Saving;

namespace Dune.SpiceAndSand
{
    /// <summary>
    /// Game entry point - Initializes all systems in correct order
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [Header("Initialization Order")]
        public bool initializeOnStart = true;
        public float initializationDelay = 0f;
        
        [Header("Managers")]
        public GameObject gameManagerPrefab;
        public GameObject audioManagerPrefab;
        public GameObject uiManagerPrefab;
        public GameObject saveSystemPrefab;
        public GameObject achievementSystemPrefab;
        public GameObject analyticsManagerPrefab;
        public GameObject networkManagerPrefab;
        
        [Header("Scene")]
        public string firstScene = "MainMenu";
        
        private void Start()
        {
            if (initializeOnStart)
            {
                StartCoroutine(InitializeGame());
            }
        }
        
        private IEnumerator InitializeGame()
        {
            // Wait for optional delay
            if (initializationDelay > 0)
            {
                yield return new WaitForSeconds(initializationDelay);
            }
            
            // Initialize core systems
            InitializeCoreSystems();
            
            // Wait for systems to initialize
            yield return new WaitForSeconds(0.1f);
            
            // Initialize managers
            InitializeManagers();
            
            // Wait for managers
            yield return new WaitForSeconds(0.1f);
            
            // Initialize UI
            InitializeUI();
            
            // Load first scene
            yield return new WaitForSeconds(0.5f);
            
            // Check for save file and load
            CheckSaveFile();
            
            // Load first scene
            LoadingScreenManager.Instance?.LoadScene(firstScene);
        }
        
        private void InitializeCoreSystems()
        {
            Debug.Log("Initializing Dune: Spice & Sand - Arrakis Strategy");
            Debug.Log("Lore fidelity: Frank Herbert's original Dune series");
            
            // Set up application settings
            Application.targetFrameRate = 60;
            Application.runInBackground = false;
            
            // Set quality settings for mobile
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadowCascades = 2;
            QualitySettings.shadowDistance = 50f;
            
            // Set input for mobile
            Input.multiTouchEnabled = true;
        }
        
        private void InitializeManagers()
        {
            // GameManager
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
                Debug.Log("GameManager initialized");
            }
            
            // AudioManager
            if (AudioManager.Instance == null && audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
                Debug.Log("AudioManager initialized");
            }
            
            // UIManager
            if (UIManager.Instance == null && uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
                Debug.Log("UIManager initialized");
            }
            
            // SaveSystem
            if (SaveSystem.Instance == null && saveSystemPrefab != null)
            {
                Instantiate(saveSystemPrefab);
                Debug.Log("SaveSystem initialized");
            }
            
            // AchievementSystem
            if (AchievementSystem.Instance == null && achievementSystemPrefab != null)
            {
                Instantiate(achievementSystemPrefab);
                Debug.Log("AchievementSystem initialized");
            }
            
            // AnalyticsManager
            if (AnalyticsManager.Instance == null && analyticsManagerPrefab != null)
            {
                Instantiate(analyticsManagerPrefab);
                Debug.Log("AnalyticsManager initialized");
            }
            
            // NetworkManager
            if (NetworkManager.Instance == null && networkManagerPrefab != null)
            {
                Instantiate(networkManagerPrefab);
                Debug.Log("NetworkManager initialized");
            }
        }
        
        private void InitializeUI()
        {
            // Ensure canvas exists
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
        
        private void CheckSaveFile()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
            {
                Debug.Log("Save file found. Loading...");
                SaveSystem.Instance.LoadGame();
            }
            else
            {
                Debug.Log("No save file found. Starting fresh.");
            }
        }
        
        public void ForceReinitialize()
        {
            StartCoroutine(InitializeGame());
        }
        
        private void OnDestroy()
        {
            // Save settings before quitting
            GameSettings.Instance?.SaveSettings();
            SaveSystem.Instance?.SaveGame();
        }
        
        private void OnApplicationQuit()
        {
            // Final cleanup
            GameSettings.Instance?.SaveSettings();
            SaveSystem.Instance?.SaveGame();
            AnalyticsManager.Instance?.TrackEvent("application_quit", null);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Save game when paused
                SaveSystem.Instance?.SaveGame();
                GameSettings.Instance?.SaveSettings();
            }
        }
    }
}
