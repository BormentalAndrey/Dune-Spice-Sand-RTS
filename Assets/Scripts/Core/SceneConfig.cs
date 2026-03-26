using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Scene configuration and management system
    /// References: Dune campaign progression
    /// </summary>
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Dune/Scene Configuration")]
    public class SceneConfig : ScriptableObject
    {
        [Header("Scenes")]
        public SceneData mainMenu;
        public List<SceneData> campaignScenes;
        public SceneData skirmishScene;
        public SceneData multiplayerScene;
        
        [Header("Loading")]
        public float defaultLoadDelay = 0.5f;
        public bool showLoadingScreen = true;
        
        [System.Serializable]
        public class SceneData
        {
            public string sceneName;
            public string displayName;
            public Sprite previewImage;
            public string description;
            public bool isUnlocked = true;
            public int requiredMissionComplete = 0;
        }
        
        private static SceneConfig _instance;
        public static SceneConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<SceneConfig>("SceneConfig");
                    if (_instance == null)
                    {
                        Debug.LogError("SceneConfig not found in Resources!");
                    }
                }
                return _instance;
            }
        }
        
        public void LoadScene(string sceneName)
        {
            if (showLoadingScreen && LoadingScreenManager.Instance != null)
            {
                LoadingScreenManager.Instance.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        
        public void LoadCampaignMission(int missionIndex)
        {
            if (missionIndex >= 0 && missionIndex < campaignScenes.Count)
            {
                SceneData mission = campaignScenes[missionIndex];
                if (mission.isUnlocked)
                {
                    LoadScene(mission.sceneName);
                }
                else
                {
                    Debug.LogWarning($"Mission {missionIndex} is locked");
                    NotificationSystem.Instance?.ShowMessage(
                        $"Complete Mission {requiredMissionComplete} to unlock this mission",
                        NotificationSystem.NotificationType.Warning);
                }
            }
        }
        
        public void UnlockMission(int missionIndex)
        {
            if (missionIndex >= 0 && missionIndex < campaignScenes.Count)
            {
                campaignScenes[missionIndex].isUnlocked = true;
                
                // Save unlocked missions
                PlayerPrefs.SetInt($"Mission_{missionIndex}_Unlocked", 1);
                PlayerPrefs.Save();
            }
        }
        
        public void LoadSavedGame()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
            {
                SaveSystem.Instance.LoadGame();
                int currentMission = SaveSystem.Instance.GetCurrentSave()?.currentMission ?? 1;
                LoadCampaignMission(currentMission - 1);
            }
            else
            {
                LoadCampaignMission(0);
            }
        }
        
        public string GetSceneDisplayName(string sceneName)
        {
            // Check main menu
            if (mainMenu.sceneName == sceneName)
                return mainMenu.displayName;
                
            // Check campaign scenes
            foreach (var scene in campaignScenes)
            {
                if (scene.sceneName == sceneName)
                    return scene.displayName;
            }
            
            // Check skirmish
            if (skirmishScene.sceneName == sceneName)
                return skirmishScene.displayName;
                
            // Check multiplayer
            if (multiplayerScene.sceneName == sceneName)
                return multiplayerScene.displayName;
                
            return sceneName;
        }
        
        public Sprite GetScenePreview(string sceneName)
        {
            if (mainMenu.sceneName == sceneName)
                return mainMenu.previewImage;
                
            foreach (var scene in campaignScenes)
            {
                if (scene.sceneName == sceneName)
                    return scene.previewImage;
            }
            
            if (skirmishScene.sceneName == sceneName)
                return skirmishScene.previewImage;
                
            if (multiplayerScene.sceneName == sceneName)
                return multiplayerScene.previewImage;
                
            return null;
        }
    }
}
