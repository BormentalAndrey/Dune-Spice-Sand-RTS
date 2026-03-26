#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dune.SpiceAndSand.Editor
{
    /// <summary>
    /// Final build verification script - Ensures all systems are ready for release
    /// </summary>
    public class BuildVerification : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("=== Dune: Spice & Sand Build Verification ===");
            
            // Check for required scenes
            CheckScenes();
            
            // Check for required components
            CheckComponents();
            
            // Check Android settings
            CheckAndroidSettings();
            
            // Check for missing references
            CheckMissingReferences();
            
            // Check performance settings
            CheckPerformanceSettings();
            
            Debug.Log("Build verification complete!");
        }
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build successful! Size: {report.summary.totalSize / (1024f * 1024f):F2} MB");
                Debug.Log($"Output: {report.summary.outputPath}");
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.result}");
            }
        }
        
        private void CheckScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            
            string[] requiredScenes = new[]
            {
                "MainMenu",
                "Campaign_Mission1",
                "Campaign_Mission2",
                "Campaign_Mission3",
                "Skirmish"
            };
            
            foreach (string requiredScene in requiredScenes)
            {
                bool found = false;
                foreach (var scene in scenes)
                {
                    if (scene.path.Contains(requiredScene))
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    Debug.LogWarning($"Missing required scene: {requiredScene}");
                }
                else
                {
                    Debug.Log($"✓ Scene found: {requiredScene}");
                }
            }
        }
        
        private void CheckComponents()
        {
            // Check for GameManager
            if (Resources.FindObjectsOfTypeAll<GameManager>().Length == 0)
            {
                Debug.LogWarning("GameManager prefab not found in Resources!");
            }
            else
            {
                Debug.Log("✓ GameManager found");
            }
            
            // Check for PrefabReferences
            PrefabReferences references = Resources.Load<PrefabReferences>("PrefabReferences");
            if (references == null)
            {
                Debug.LogWarning("PrefabReferences not found in Resources!");
            }
            else
            {
                Debug.Log("✓ PrefabReferences found");
            }
        }
        
        private void CheckAndroidSettings()
        {
            // Check package name
            if (string.IsNullOrEmpty(PlayerSettings.applicationIdentifier))
            {
                Debug.LogWarning("Android package name not set!");
            }
            else
            {
                Debug.Log($"✓ Package name: {PlayerSettings.applicationIdentifier}");
            }
            
            // Check minimum API level
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel26)
            {
                Debug.LogWarning("Minimum API level below Android 8.0 (API 26)!");
            }
            else
            {
                Debug.Log($"✓ Min API: {PlayerSettings.Android.minSdkVersion}");
            }
            
            // Check target architecture
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
            {
                Debug.LogWarning("ARM64 architecture not selected!");
            }
            else
            {
                Debug.Log("✓ ARM64 architecture enabled");
            }
            
            // Check IL2CPP
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                Debug.LogWarning("Scripting backend should be IL2CPP for Android!");
            }
            else
            {
                Debug.Log("✓ IL2CPP scripting backend");
            }
        }
        
        private void CheckMissingReferences()
        {
            // Check for missing script references in scenes
            GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            
            foreach (var root in rootObjects)
            {
                MonoBehaviour[] scripts = root.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var script in scripts)
                {
                    if (script == null)
                    {
                        Debug.LogWarning($"Missing script reference in {root.name}");
                    }
                }
            }
            
            Debug.Log("✓ Missing reference check complete");
        }
        
        private void CheckPerformanceSettings()
        {
            // Check quality settings
            if (QualitySettings.vSyncCount > 0)
            {
                Debug.Log("VSync enabled - consider disabling for mobile");
            }
            
            // Check shadow settings
            if (QualitySettings.shadowDistance > 100f)
            {
                Debug.LogWarning("Shadow distance too high for mobile - consider reducing");
            }
            
            Debug.Log("✓ Performance settings checked");
        }
    }
    
    /// <summary>
    /// Build size report tool
    /// </summary>
    public class BuildSizeReport : EditorWindow
    {
        [MenuItem("Dune/Reports/Build Size Report")]
        public static void ShowWindow()
        {
            GetWindow<BuildSizeReport>("Build Size Report");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Build Size Analysis", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Analyze Current Build"))
            {
                AnalyzeBuild();
            }
        }
        
        private void AnalyzeBuild()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D");
            long totalTextureSize = 0;
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer != null)
                {
                    // Calculate approximate size
                    long size = GetTextureSize(importer);
                    totalTextureSize += size;
                }
            }
            
            Debug.Log($"Total texture memory estimate: {totalTextureSize / (1024f * 1024f):F2} MB");
        }
        
        private long GetTextureSize(TextureImporter importer)
        {
            // Estimate texture size based on dimensions and format
            // Simplified calculation
            return 1024 * 1024 * 4; // 4MB placeholder
        }
    }
}
#endif
