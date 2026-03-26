using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Configuration script for UI prefabs - Mobile-optimized layout
    /// References: Dune-inspired UI design
    /// </summary>
    public class UIPrefabConfig : MonoBehaviour
    {
        [Header("Screen Settings")]
        public ScreenOrientation defaultOrientation = ScreenOrientation.LandscapeLeft;
        public bool allowRotation = true;
        
        [Header("Font Settings")]
        public TMP_FontAsset mainFont;
        public TMP_FontAsset titleFont;
        public TMP_FontAsset desertFont;
        
        [Header("Color Scheme")]
        public Color primaryColor = new Color(0.85f, 0.65f, 0.25f); // Sand gold
        public Color secondaryColor = new Color(0.4f, 0.25f, 0.1f); // Desert brown
        public Color accentColor = new Color(0.9f, 0.3f, 0.1f); // Spice orange
        public Color dangerColor = new Color(0.9f, 0.2f, 0.2f);
        public Color successColor = new Color(0.2f, 0.7f, 0.3f);
        public Color warningColor = new Color(0.9f, 0.7f, 0.2f);
        
        [Header("Button Sizes")]
        public float smallButtonSize = 80f;
        public float mediumButtonSize = 120f;
        public float largeButtonSize = 160f;
        public float buttonSpacing = 20f;
        
        [Header("Panel Settings")]
        public float panelPadding = 20f;
        public float panelCornerRadius = 10f;
        public float panelOpacity = 0.9f;
        
        [Header("Text Sizes")]
        public float titleTextSize = 48f;
        public float headerTextSize = 32f;
        public float bodyTextSize = 24f;
        public float smallTextSize = 18f;
        
        private static UIPrefabConfig _instance;
        public static UIPrefabConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIPrefabConfig>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("UIPrefabConfig");
                        _instance = go.AddComponent<UIPrefabConfig>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            ApplyScreenSettings();
        }
        
        private void ApplyScreenSettings()
        {
            Screen.orientation = defaultOrientation;
            Screen.autorotateToLandscapeLeft = allowRotation;
            Screen.autorotateToLandscapeRight = allowRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }
        
        public void StyleButton(Button button, ButtonSize size = ButtonSize.Medium)
        {
            if (button == null) return;
            
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                switch (size)
                {
                    case ButtonSize.Small:
                        rect.sizeDelta = new Vector2(smallButtonSize, smallButtonSize);
                        break;
                    case ButtonSize.Medium:
                        rect.sizeDelta = new Vector2(mediumButtonSize, mediumButtonSize);
                        break;
                    case ButtonSize.Large:
                        rect.sizeDelta = new Vector2(largeButtonSize, largeButtonSize);
                        break;
                }
            }
            
            // Style button colors
            ColorBlock colors = button.colors;
            colors.normalColor = primaryColor;
            colors.highlightedColor = accentColor;
            colors.pressedColor = secondaryColor;
            colors.selectedColor = primaryColor;
            button.colors = colors;
        }
        
        public void StyleText(TextMeshProUGUI text, TextType type = TextType.Body)
        {
            if (text == null) return;
            
            if (mainFont != null) text.font = mainFont;
            
            switch (type)
            {
                case TextType.Title:
                    text.fontSize = titleTextSize;
                    text.fontStyle = FontStyles.Bold;
                    text.color = accentColor;
                    break;
                case TextType.Header:
                    text.fontSize = headerTextSize;
                    text.fontStyle = FontStyles.Bold;
                    text.color = primaryColor;
                    break;
                case TextType.Body:
                    text.fontSize = bodyTextSize;
                    text.fontStyle = FontStyles.Normal;
                    text.color = Color.white;
                    break;
                case TextType.Small:
                    text.fontSize = smallTextSize;
                    text.fontStyle = FontStyles.Normal;
                    text.color = Color.gray;
                    break;
            }
        }
        
        public void StylePanel(RectTransform panel)
        {
            // Apply corner radius via image
            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = panelOpacity;
                image.color = color;
            }
        }
        
        public enum ButtonSize
        {
            Small,
            Medium,
            Large
        }
        
        public enum TextType
        {
            Title,
            Header,
            Body,
            Small
        }
    }
    
    /// <summary>
    /// Resource bar configuration
    /// </summary>
    public class ResourceBarConfig : MonoBehaviour
    {
        [Header("Resource Bars")]
        public Slider spiceSlider;
        public Slider waterSlider;
        public Slider jihadSlider;
        
        [Header("Text")]
        public TextMeshProUGUI spiceText;
        public TextMeshProUGUI waterText;
        public TextMeshProUGUI jihadText;
        
        [Header("Colors")]
        public Color spiceColor = new Color(1f, 0.7f, 0.2f);
        public Color waterColor = new Color(0.2f, 0.5f, 0.8f);
        public Color jihadColor = new Color(0.8f, 0.2f, 0.2f);
        
        private void Start()
        {
            if (spiceSlider != null)
            {
                Image fill = spiceSlider.fillRect.GetComponent<Image>();
                if (fill != null) fill.color = spiceColor;
            }
            
            if (waterSlider != null)
            {
                Image fill = waterSlider.fillRect.GetComponent<Image>();
                if (fill != null) fill.color = waterColor;
            }
            
            if (jihadSlider != null)
            {
                Image fill = jihadSlider.fillRect.GetComponent<Image>();
                if (fill != null) fill.color = jihadColor;
            }
        }
        
        public void UpdateResources(float spice, float water, float spiceMax, float waterMax)
        {
            if (spiceSlider != null)
                spiceSlider.value = spice / spiceMax;
            if (spiceText != null)
                spiceText.text = $"🌶️ {spice:F0} / {spiceMax:F0}";
                
            if (waterSlider != null)
                waterSlider.value = water / waterMax;
            if (waterText != null)
                waterText.text = $"💧 {water:F0} / {waterMax:F0}";
        }
        
        public void UpdateJihad(float value, float max)
        {
            if (jihadSlider != null)
                jihadSlider.value = value / max;
            if (jihadText != null)
                jihadText.text = $"Jihad: {value:F0}%";
        }
    }
}
