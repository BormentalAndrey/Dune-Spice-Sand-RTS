using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dune.SpiceAndSand.Core
{
    /// <summary>
    /// Game settings manager for preferences and options
    /// References: Dune terminology in UI
    /// </summary>
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance { get; private set; }
        
        [Header("Graphics Settings")]
        public int qualityLevel = 2; // 0=Low, 1=Medium, 2=High, 3=Ultra
        public int shadowQuality = 2;
        public int textureQuality = 2;
        public int antiAliasing = 2;
        public float renderScale = 1f;
        public bool vSyncEnabled = false;
        public int targetFrameRate = 60;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)] public float masterVolume = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        [Range(0f, 1f)] public float ambientVolume = 0.4f;
        [Range(0f, 1f)] public float voiceVolume = 0.8f;
        
        [Header("Gameplay Settings")]
        public bool cameraShake = true;
        public bool tooltipsEnabled = true;
        public bool autoSaveEnabled = true;
        public float autoSaveInterval = 300f;
        public bool showDamageNumbers = true;
        public bool showFloatingText = true;
        
        [Header("Accessibility")]
        public bool colorBlindMode = false;
        public ColorBlindType colorBlindType = ColorBlindType.Normal;
        public bool largeTextMode = false;
        public float textScale = 1f;
        public bool highContrastMode = false;
        
        [Header("Control Settings")]
        public float cameraPanSpeed = 20f;
        public float cameraZoomSpeed = 10f;
        public float cameraRotationSpeed = 100f;
        public bool invertCameraY = false;
        public float edgeScrollThreshold = 50f;
        public bool edgeScrollingEnabled = true;
        
        [Header("Performance")]
        public bool dynamicResolutionEnabled = true;
        public bool objectPoolingEnabled = true;
        public int maxVisibleUnits = 50;
        public float lodDistance = 50f;
        
        public enum ColorBlindType
        {
            Normal,
            Protanopia,  // Red-blind
            Deuteranopia, // Green-blind
            Tritanopia    // Blue-blind
        }
        
        private string settingsFilePath;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            settingsFilePath = Path.Combine(Application.persistentDataPath, "game_settings.dat");
            LoadSettings();
        }
        
        private void Start()
        {
            ApplySettings();
        }
        
        public void ApplySettings()
        {
            // Apply graphics
            QualitySettings.SetQualityLevel(qualityLevel);
            QualitySettings.shadows = (ShadowQuality)shadowQuality;
            QualitySettings.masterTextureLimit = 3 - textureQuality;
            QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
            
            // Apply frame rate
            Application.targetFrameRate = targetFrameRate;
            
            // Apply audio
            AudioListener.volume = masterVolume;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(musicVolume);
                AudioManager.Instance.SetSFXVolume(sfxVolume);
                AudioManager.Instance.SetAmbientVolume(ambientVolume);
                AudioManager.Instance.SetVoiceVolume(voiceVolume);
            }
            
            // Apply accessibility
            if (colorBlindMode)
            {
                ApplyColorBlindFilter();
            }
            
            // Apply performance
            if (PerformanceOptimizer.Instance != null)
            {
                // Performance settings applied in optimizer
            }
        }
        
        private void ApplyColorBlindFilter()
        {
            // Apply color correction for color blindness
            // Dune: Spice & Sand accessibility feature
            switch (colorBlindType)
            {
                case ColorBlindType.Protanopia:
                    // Apply red-blind filter
                    break;
                case ColorBlindType.Deuteranopia:
                    // Apply green-blind filter
                    break;
                case ColorBlindType.Tritanopia:
                    // Apply blue-blind filter
                    break;
            }
        }
        
        public void SaveSettings()
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(settingsFilePath, FileMode.Create);
                formatter.Serialize(stream, this);
                stream.Close();
                Debug.Log("Settings saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save settings: {e.Message}");
            }
        }
        
        public void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(settingsFilePath, FileMode.Open);
                    GameSettings loaded = (GameSettings)formatter.Deserialize(stream);
                    stream.Close();
                    
                    // Copy loaded settings
                    CopySettings(loaded);
                    Debug.Log("Settings loaded");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load settings: {e.Message}");
                    SetDefaultSettings();
                }
            }
            else
            {
                SetDefaultSettings();
            }
        }
        
        private void CopySettings(GameSettings other)
        {
            qualityLevel = other.qualityLevel;
            shadowQuality = other.shadowQuality;
            textureQuality = other.textureQuality;
            antiAliasing = other.antiAliasing;
            renderScale = other.renderScale;
            vSyncEnabled = other.vSyncEnabled;
            targetFrameRate = other.targetFrameRate;
            
            masterVolume = other.masterVolume;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            ambientVolume = other.ambientVolume;
            voiceVolume = other.voiceVolume;
            
            cameraShake = other.cameraShake;
            tooltipsEnabled = other.tooltipsEnabled;
            autoSaveEnabled = other.autoSaveEnabled;
            autoSaveInterval = other.autoSaveInterval;
            
            colorBlindMode = other.colorBlindMode;
            colorBlindType = other.colorBlindType;
            largeTextMode = other.largeTextMode;
            textScale = other.textScale;
            
            cameraPanSpeed = other.cameraPanSpeed;
            cameraZoomSpeed = other.cameraZoomSpeed;
            cameraRotationSpeed = other.cameraRotationSpeed;
            invertCameraY = other.invertCameraY;
            edgeScrollingEnabled = other.edgeScrollingEnabled;
        }
        
        private void SetDefaultSettings()
        {
            qualityLevel = 2;
            shadowQuality = 2;
            textureQuality = 2;
            antiAliasing = 2;
            renderScale = 1f;
            vSyncEnabled = false;
            targetFrameRate = 60;
            
            masterVolume = 0.8f;
            musicVolume = 0.5f;
            sfxVolume = 0.7f;
            ambientVolume = 0.4f;
            voiceVolume = 0.8f;
            
            cameraShake = true;
            tooltipsEnabled = true;
            autoSaveEnabled = true;
            autoSaveInterval = 300f;
            
            colorBlindMode = false;
            largeTextMode = false;
            textScale = 1f;
            
            cameraPanSpeed = 20f;
            cameraZoomSpeed = 10f;
            cameraRotationSpeed = 100f;
            invertCameraY = false;
            edgeScrollingEnabled = true;
        }
        
        public void ResetToDefaults()
        {
            SetDefaultSettings();
            ApplySettings();
            SaveSettings();
        }
        
        private void OnApplicationQuit()
        {
            SaveSettings();
        }
        
        public void SetQualityPreset(string preset)
        {
            switch (preset.ToLower())
            {
                case "low":
                    qualityLevel = 0;
                    shadowQuality = 0;
                    textureQuality = 0;
                    antiAliasing = 0;
                    targetFrameRate = 30;
                    break;
                case "medium":
                    qualityLevel = 1;
                    shadowQuality = 1;
                    textureQuality = 1;
                    antiAliasing = 2;
                    targetFrameRate = 60;
                    break;
                case "high":
                    qualityLevel = 2;
                    shadowQuality = 2;
                    textureQuality = 2;
                    antiAliasing = 4;
                    targetFrameRate = 60;
                    break;
                case "ultra":
                    qualityLevel = 3;
                    shadowQuality = 3;
                    textureQuality = 3;
                    antiAliasing = 8;
                    targetFrameRate = 60;
                    break;
            }
            ApplySettings();
        }
    }
}
