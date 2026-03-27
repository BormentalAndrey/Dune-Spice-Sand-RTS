using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Integration
{
    /// <summary>
    /// Checks for game updates and displays notifications
    /// References: Version management for Dune game
    /// </summary>
    public class UpdateChecker : MonoBehaviour
    {
        public static UpdateChecker Instance { get; private set; }
        
        [Header("Update Settings")]
        public string updateUrl = "https://api.dunespiceandsand.com/version";
        public float checkInterval = 86400f; // 24 hours
        public bool checkOnStart = true;
        
        [Header("UI")]
        public GameObject updateNotificationPanel;
        public TextMeshProUGUI updateMessageText;
        public Button updateButton;
        public Button dismissButton;
        
        [Header("Google Play")]
        public string googlePlayAppId = "com.yourcompany.dunespiceandsand";
        
        private VersionInfo latestVersion;
        private bool isChecking = false;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (checkOnStart)
            {
                CheckForUpdates();
            }
            
            StartCoroutine(PeriodicCheck());
        }
        
        private IEnumerator PeriodicCheck()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);
                CheckForUpdates();
            }
        }
        
        public void CheckForUpdates()
        {
            if (isChecking) return;
            StartCoroutine(CheckRoutine());
        }
        
        private IEnumerator CheckRoutine()
        {
            isChecking = true;
            
            using (UnityWebRequest request = UnityWebRequest.Get(updateUrl))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        UpdateResponse response = JsonUtility.FromJson<UpdateResponse>(request.downloadHandler.text);
                        latestVersion = new VersionInfo
                        {
                            majorVersion = response.majorVersion,
                            minorVersion = response.minorVersion,
                            patchVersion = response.patchVersion,
                            buildNumber = response.buildNumber,
                            releaseName = response.releaseName,
                            updateUrl = response.downloadUrl
                        };
                        
                        // Check if update is available
                        if (IsNewerVersionAvailable(latestVersion))
                        {
                            ShowUpdateNotification(latestVersion);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to parse update response: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to check for updates: {request.error}");
                }
            }
            
            isChecking = false;
        }
        
        private bool IsNewerVersionAvailable(VersionInfo remote)
        {
            VersionInfo local = VersionInfo.Instance;
            
            if (remote.majorVersion > local.majorVersion)
                return true;
            if (remote.majorVersion == local.majorVersion && remote.minorVersion > local.minorVersion)
                return true;
            if (remote.majorVersion == local.majorVersion && remote.minorVersion == local.minorVersion && 
                remote.patchVersion > local.patchVersion)
                return true;
                
            return false;
        }
        
        private void ShowUpdateNotification(VersionInfo version)
        {
            if (updateNotificationPanel == null) return;
            
            string message = $"A new version of Dune: Spice & Sand is available!\n\n" +
                            $"Current: {VersionInfo.Instance.GetDisplayVersion()}\n" +
                            $"New: v{version.majorVersion}.{version.minorVersion}.{version.patchVersion}\n\n" +
                            $"{version.releaseName}\n\n" +
                            $"Would you like to update now?";
            
            if (updateMessageText != null)
                updateMessageText.text = message;
            
            updateNotificationPanel.SetActive(true);
            
            if (updateButton != null)
                updateButton.onClick.AddListener(OpenUpdatePage);
            
            if (dismissButton != null)
                dismissButton.onClick.AddListener(() => updateNotificationPanel.SetActive(false));
        }
        
        public void OpenUpdatePage()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            Application.OpenURL($"market://details?id={googlePlayAppId}");
            #else
            Application.OpenURL($"https://play.google.com/store/apps/details?id={googlePlayAppId}");
            #endif
        }
        
        [System.Serializable]
        public class UpdateResponse
        {
            public int majorVersion;
            public int minorVersion;
            public int patchVersion;
            public int buildNumber;
            public string releaseName;
            public string downloadUrl;
            public bool isMandatory;
            public string releaseNotes;
        }
        
        public class VersionInfo
        {
            public int majorVersion;
            public int minorVersion;
            public int patchVersion;
            public int buildNumber;
            public string releaseName;
            public string updateUrl;
            
            public string GetDisplayVersion()
            {
                return $"v{majorVersion}.{minorVersion}.{patchVersion}";
            }
        }
    }
}
