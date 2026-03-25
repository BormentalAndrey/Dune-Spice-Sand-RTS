using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.UI;

namespace Dune.SpiceAndSand.Setup
{
    /// <summary>
    /// Scene setup and initialization for all game scenes
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Scene Settings")]
        public SceneType currentSceneType;
        
        [Header("Managers")]
        public GameObject gameManagerPrefab;
        public GameObject uiManagerPrefab;
        public GameObject audioManagerPrefab;
        public GameObject settingsManagerPrefab;
        
        public enum SceneType
        {
            MainMenu,
            Campaign,
            Skirmish,
            Multiplayer
        }
        
        private void Awake()
        {
            // Ensure managers exist
            EnsureManagersExist();
            
            // Scene-specific setup
            SetupScene();
        }
        
        private void EnsureManagersExist()
        {
            // GameManager
            if (GameManager.Instance == null && gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            
            // UIManager
            if (UIManager.Instance == null && uiManagerPrefab != null)
            {
                Instantiate(uiManagerPrefab);
            }
            
            // AudioManager
            if (AudioManager.Instance == null && audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
            }
            
            // GameSettings
            if (GameSettings.Instance == null && settingsManagerPrefab != null)
            {
                Instantiate(settingsManagerPrefab);
            }
        }
        
        private void SetupScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            
            if (sceneName.Contains("MainMenu"))
            {
                SetupMainMenu();
            }
            else if (sceneName.Contains("Campaign"))
            {
                SetupCampaign(sceneName);
            }
            else if (sceneName.Contains("Skirmish"))
            {
                SetupSkirmish();
            }
            else if (sceneName.Contains("Multiplayer"))
            {
                SetupMultiplayer();
            }
        }
        
        private void SetupMainMenu()
        {
            currentSceneType = SceneType.MainMenu;
            
            // Play main menu music
            if (AudioManager.Instance != null)
            {
                // AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic);
            }
            
            // Show cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
        private void SetupCampaign(string sceneName)
        {
            currentSceneType = SceneType.Campaign;
            
            // Extract mission number from scene name
            int missionNumber = 1;
            string missionNumberStr = sceneName.Replace("Campaign_Mission", "");
            if (int.TryParse(missionNumberStr, out missionNumber))
            {
                // Load mission data
                StartCoroutine(LoadMissionData(missionNumber));
            }
            
            // Setup camera
            SetupCamera();
            
            // Setup terrain
            SetupTerrain();
        }
        
        private IEnumerator LoadMissionData(int missionNumber)
        {
            yield return null;
            
            // Wait for CampaignManager
            while (CampaignManager.Instance == null)
            {
                yield return null;
            }
            
            // Set mission
            CampaignManager.Instance.currentMission = missionNumber;
            
            // Get mission objectives
            MissionObjective objectiveSystem = FindObjectOfType<MissionObjective>();
            if (objectiveSystem != null)
            {
                switch (missionNumber)
                {
                    case 1:
                        objectiveSystem.objectives = MissionObjective.GetMission1Objectives();
                        break;
                    case 2:
                        objectiveSystem.objectives = MissionObjective.GetMission2Objectives();
                        break;
                    case 3:
                        objectiveSystem.objectives = MissionObjective.GetMission3Objectives();
                        break;
                }
            }
            
            // Show mission briefing
            ShowMissionBriefing(missionNumber);
        }
        
        private void ShowMissionBriefing(int missionNumber)
        {
            DialogueSystem dialogue = FindObjectOfType<DialogueSystem>();
            if (dialogue != null)
            {
                switch (missionNumber)
                {
                    case 1:
