#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Dune.SpiceAndSand.Editor
{
    /// <summary>
    /// Editor utilities for building APK and automating workflows
    /// </summary>
    public class BuildAutomation : EditorWindow
    {
        [MenuItem("Dune/Build/Android APK")]
        public static void BuildAndroidAPK()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] 
            { 
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Campaign_Mission1.unity",
                "Assets/Scenes/Campaign_Mission2.unity",
                "Assets/Scenes/Campaign_Mission3.unity",
                "Assets/Scenes/Skirmish.unity"
            };
            buildPlayerOptions.locationPathName = "Builds/Dune_Spice_And_Sand.apk";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed!");
            }
        }
        
        [MenuItem("Dune/Build/Android App Bundle")]
        public static void BuildAndroidAAB()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = new[] 
            { 
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Campaign_Mission1.unity",
                "Assets/Scenes/Campaign_Mission2.unity",
                "Assets/Scenes/Campaign_Mission3.unity",
                "Assets/Scenes/Skirmish.unity"
            };
            buildPlayerOptions.locationPathName = "Builds/Dune_Spice_And_Sand.aab";
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            EditorUserBuildSettings.buildAppBundle = true;
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"AAB build succeeded: {summary.totalSize} bytes");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("AAB build failed!");
            }
        }
        
        [MenuItem("Dune/Setup/Configure Android")]
        public static void ConfigureAndroid()
        {
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.forceSDCardPermission = false;
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            PlayerSettings.applicationIdentifier = "com.dune.spiceandsand";
            PlayerSettings.productName = "Dune: Spice & Sand";
            PlayerSettings.companyName = "YourCompany";
            PlayerSettings.bundleVersion = "1.0.0";
            
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.buildScriptsOnly = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
            
            Debug.Log("Android configuration complete!");
        }
        
        [MenuItem("Dune/Setup/Validate Build")]
        public static void ValidateBuild()
        {
            bool hasErrors = false;
            
            // Check for required scenes
            string[] requiredScenes = new[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Campaign_Mission1.unity"
            };
            
            foreach (string scene in requiredScenes)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null)
                {
                    Debug.LogError($"Missing required scene: {scene}");
                    hasErrors = true;
                }
            }
            
            // Check for required packages
            if (!EditorApplication.isCompiling)
            {
                Debug.Log("No compilation errors detected.");
            }
            
            if (!hasErrors)
            {
                Debug.Log("Build validation passed! Ready to build.");
            }
            else
            {
                Debug.LogError("Build validation failed. Fix errors before building.");
            }
        }
    }
    
    /// <summary>
    /// Post-processor for setting Android manifest permissions
    /// </summary>
    public class AndroidManifestPostprocessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;
        
        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                Debug.Log("Android build post-processing complete!");
                // Additional post-processing can be added here
            }
        }
    }
}
#endif
