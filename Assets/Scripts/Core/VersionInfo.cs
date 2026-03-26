using UnityEngine;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Version information for the game
    /// References: Build tracking and updates
    /// </summary>
    [CreateAssetMenu(fileName = "VersionInfo", menuName = "Dune/Version Info")]
    public class VersionInfo : ScriptableObject
    {
        [Header("Version Numbers")]
        public int majorVersion = 1;
        public int minorVersion = 0;
        public int patchVersion = 0;
        public int buildNumber = 1;
        
        [Header("Release Info")]
        public string releaseName = "The Spice Must Flow";
        public System.DateTime releaseDate = new System.DateTime(2024, 1, 15);
        
        [Header("Compatibility")]
        public int minimumAndroidAPI = 26;
        public int targetAndroidAPI = 33;
        
        [Header("Update Info")]
        public string updateUrl = "https://dunespiceandsand.com/update";
        public bool forceUpdate = false;
        
        private static VersionInfo _instance;
        public static VersionInfo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<VersionInfo>("VersionInfo");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<VersionInfo>();
                    }
                }
                return _instance;
            }
        }
        
        public string GetVersionString()
        {
            return $"{majorVersion}.{minorVersion}.{patchVersion}.{buildNumber}";
        }
        
        public string GetDisplayVersion()
        {
            return $"v{majorVersion}.{minorVersion}.{patchVersion}";
        }
        
        public string GetFullVersionName()
        {
            return $"{GetDisplayVersion()} - {releaseName}";
        }
        
        public string GetBuildDate()
        {
            return releaseDate.ToString("yyyy-MM-dd");
        }
        
        public bool IsNewerThan(VersionInfo other)
        {
            if (majorVersion != other.majorVersion)
                return majorVersion > other.majorVersion;
            if (minorVersion != other.minorVersion)
                return minorVersion > other.minorVersion;
            if (patchVersion != other.patchVersion)
                return patchVersion > other.patchVersion;
            return buildNumber > other.buildNumber;
        }
        
        public bool IsCompatibleWithAndroid(int apiLevel)
        {
            return apiLevel >= minimumAndroidAPI;
        }
        
        public void LogVersionInfo()
        {
            Debug.Log("=== Dune: Spice & Sand Version Info ===");
            Debug.Log($"Version: {GetFullVersionName()}");
            Debug.Log($"Build: {GetVersionString()}");
            Debug.Log($"Release Date: {GetBuildDate()}");
            Debug.Log($"Android API: {minimumAndroidAPI}+ (Target: {targetAndroidAPI})");
            Debug.Log("=======================================");
        }
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Dune/Version/Increment Build Number")]
        public static void IncrementBuildNumber()
        {
            VersionInfo info = Instance;
            info.buildNumber++;
            UnityEditor.EditorUtility.SetDirty(info);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Build number incremented to {info.buildNumber}");
        }
        
        [UnityEditor.MenuItem("Dune/Version/Increment Patch Version")]
        public static void IncrementPatchVersion()
        {
            VersionInfo info = Instance;
            info.patchVersion++;
            info.buildNumber = 1;
            UnityEditor.EditorUtility.SetDirty(info);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Patch version incremented to {info.patchVersion}");
        }
        
        [UnityEditor.MenuItem("Dune/Version/Increment Minor Version")]
        public static void IncrementMinorVersion()
        {
            VersionInfo info = Instance;
            info.minorVersion++;
            info.patchVersion = 0;
            info.buildNumber = 1;
            UnityEditor.EditorUtility.SetDirty(info);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Minor version incremented to {info.minorVersion}");
        }
        
        [UnityEditor.MenuItem("Dune/Version/Increment Major Version")]
        public static void IncrementMajorVersion()
        {
            VersionInfo info = Instance;
            info.majorVersion++;
            info.minorVersion = 0;
            info.patchVersion = 0;
            info.buildNumber = 1;
            UnityEditor.EditorUtility.SetDirty(info);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Major version incremented to {info.majorVersion}");
        }
        #endif
    }
}
