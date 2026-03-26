using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Settings menu with all game options
    /// </summary>
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;
        
        [Header("Graphics")]
        public TMP_Dropdown qualityDropdown;
        public TMP_Dropdown resolutionDropdown;
        public Slider brightnessSlider;
        public Toggle vSyncToggle;
        public Toggle fpsCounterToggle;
        
        [Header("Audio")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider voiceVolumeSlider;
        
        [Header("Gameplay")]
        public Toggle cameraShakeToggle;
        public Toggle tooltipsToggle;
        public Toggle autoSaveToggle;
        public Slider autoSaveIntervalSlider;
        
        [Header("Accessibility")]
        public Toggle colorBlindToggle;
        public TMP_Dropdown colorBlindTypeDropdown;
        public Toggle largeTextToggle;
        public Slider textScaleSlider;
        
        [Header("Controls")]
        public Slider cameraPanSpeedSlider;
        public Slider cameraZoomSpeedSlider;
        public Toggle edgeScrollingToggle;
        
        [Header("Buttons")]
        public Button applyButton;
        public Button resetButton;
        public Button closeButton;
        
        private GameSettings settings;
        
        private void Start()
        {
            settings = GameSettings.Instance;
            
            // Graphics settings
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High", "Ultra" });
                qualityDropdown.value = settings.qualityLevel;
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
            
            if (brightnessSlider != null)
            {
                brightnessSlider.value = 1f;
                brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            }
            
            if (vSyncToggle != null)
            {
                vSyncToggle.isOn = settings.vSyncEnabled;
                vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }
            
            // Audio settings
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = settings.masterVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = settings.musicVolume;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = settings.sfxVolume;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.value = settings.voiceVolume;
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            }
            
            // Gameplay settings
            if (cameraShakeToggle != null)
            {
                cameraShakeToggle.isOn = settings.cameraShake;
                cameraShakeToggle.onValueChanged.AddListener(OnCameraShakeChanged);
            }
            
            if (tooltipsToggle != null)
            {
                tooltipsToggle.isOn = settings.tooltipsEnabled;
                tooltipsToggle.onValueChanged.AddListener(OnTooltipsChanged);
            }
            
            if (autoSaveToggle != null)
            {
                autoSaveToggle.isOn = settings.autoSaveEnabled;
                autoSaveToggle.onValueChanged.AddListener(OnAutoSaveChanged);
            }
            
            if (autoSaveIntervalSlider != null)
            {
                autoSaveIntervalSlider.value = settings.autoSaveInterval / 60f;
                autoSaveIntervalSlider.onValueChanged.AddListener(OnAutoSaveIntervalChanged);
            }
            
            // Accessibility
            if (colorBlindToggle != null)
            {
                colorBlindToggle.isOn = settings.colorBlindMode;
                colorBlindToggle.onValueChanged.AddListener(OnColorBlindChanged);
            }
            
            if (colorBlindTypeDropdown != null)
            {
                colorBlindTypeDropdown.ClearOptions();
                colorBlindTypeDropdown.AddOptions(new List<string> { "Normal", "Protanopia", "Deuteranopia", "Tritanopia" });
                colorBlindTypeDropdown.value = (int)settings.colorBlindType;
                colorBlindTypeDropdown.onValueChanged.AddListener(OnColorBlindTypeChanged);
            }
            
            if (largeTextToggle != null)
            {
                largeTextToggle.isOn = settings.largeTextMode;
                largeTextToggle.onValueChanged.AddListener(OnLargeTextChanged);
            }
            
            // Controls
            if (cameraPanSpeedSlider != null)
            {
                cameraPanSpeedSlider.value = settings.cameraPanSpeed / 50f;
                cameraPanSpeedSlider.onValueChanged.AddListener(OnCameraPanSpeedChanged);
            }
            
            if (cameraZoomSpeedSlider != null)
            {
                cameraZoomSpeedSlider.value = settings.cameraZoomSpeed / 20f;
                cameraZoomSpeedSlider.onValueChanged.AddListener(OnCameraZoomSpeedChanged);
            }
            
            if (edgeScrollingToggle != null)
            {
                edgeScrollingToggle.isOn = settings.edgeScrollingEnabled;
                edgeScrollingToggle.onValueChanged.AddListener(OnEdgeScrollingChanged);
            }
            
            // Buttons
            if (applyButton != null) applyButton.onClick.AddListener(ApplySettings);
            if (resetButton != null) resetButton.onClick.AddListener(ResetSettings);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }
        
        private void OnQualityChanged(int value)
        {
            settings.qualityLevel = value;
        }
        
        private void OnBrightnessChanged(float value)
        {
            // Apply brightness filter
            // Screen.brightness not supported in Unity, use post-processing
        }
        
        private void OnVSyncChanged(bool value)
        {
            settings.vSyncEnabled = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            settings.masterVolume = value;
            AudioListener.volume = value;
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            settings.musicVolume = value;
            AudioManager.Instance?.SetMusicVolume(value);
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            settings.sfxVolume = value;
            AudioManager.Instance?.SetSFXVolume(value);
        }
        
        private void OnVoiceVolumeChanged(float value)
        {
            settings.voiceVolume = value;
            AudioManager.Instance?.SetVoiceVolume(value);
        }
        
        private void OnCameraShakeChanged(bool value)
        {
            settings.cameraShake = value;
        }
        
        private void OnTooltipsChanged(bool value)
        {
            settings.tooltipsEnabled = value;
        }
        
        private void OnAutoSaveChanged(bool value)
        {
            settings.autoSaveEnabled = value;
        }
        
        private void OnAutoSaveIntervalChanged(float value)
        {
            settings.autoSaveInterval = value * 60f;
        }
        
        private void OnColorBlindChanged(bool value)
        {
            settings.colorBlindMode = value;
        }
        
        private void OnColorBlindTypeChanged(int value)
        {
            settings.colorBlindType = (GameSettings.ColorBlindType)value;
        }
        
        private void OnLargeTextChanged(bool value)
        {
            settings.largeTextMode = value;
            settings.textScale = value ? 1.5f : 1f;
        }
        
        private void OnCameraPanSpeedChanged(float value)
        {
            settings.cameraPanSpeed = value * 50f;
        }
        
        private void OnCameraZoomSpeedChanged(float value)
        {
            settings.cameraZoomSpeed = value * 20f;
        }
        
        private void OnEdgeScrollingChanged(bool value)
        {
            settings.edgeScrollingEnabled = value;
        }
        
        private void ApplySettings()
        {
            settings.ApplySettings();
            settings.SaveSettings();
            
            NotificationSystem.Instance?.ShowMessage("Settings applied", 
                NotificationSystem.NotificationType.Success, 2f);
        }
        
        private void ResetSettings()
        {
            settings.ResetToDefaults();
            
            // Refresh UI
            qualityDropdown.value = settings.qualityLevel;
            vSyncToggle.isOn = settings.vSyncEnabled;
            masterVolumeSlider.value = settings.masterVolume;
            musicVolumeSlider.value = settings.musicVolume;
            sfxVolumeSlider.value = settings.sfxVolume;
            voiceVolumeSlider.value = settings.voiceVolume;
            cameraShakeToggle.isOn = settings.cameraShake;
            tooltipsToggle.isOn = settings.tooltipsEnabled;
            autoSaveToggle.isOn = settings.autoSaveEnabled;
            colorBlindToggle.isOn = settings.colorBlindMode;
            largeTextToggle.isOn = settings.largeTextMode;
            
            NotificationSystem.Instance?.ShowMessage("Settings reset to defaults", 
                NotificationSystem.NotificationType.Info, 2f);
        }
        
        public void Show()
        {
            panel.SetActive(true);
        }
        
        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}
