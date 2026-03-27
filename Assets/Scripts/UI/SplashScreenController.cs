using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Splash screen controller with Dune lore quotes
    /// References: Dune book quotes for loading experience
    /// </summary>
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Splash Settings")]
        public float splashDuration = 3f;
        public bool skipOnTap = true;
        public string nextScene = "MainMenu";
        
        [Header("UI Elements")]
        public Image logoImage;
        public TextMeshProUGUI quoteText;
        public TextMeshProUGUI attributionText;
        public Image fadeOverlay;
        
        [Header("Quotes")]
        public string[] duneQuotes;
        public string[] quoteAttributions;
        
        [Header("Animation")]
        public float fadeInDuration = 1f;
        public float fadeOutDuration = 1f;
        public float logoScaleDuration = 1.5f;
        
        private bool isSkipped = false;
        
        private void Start()
        {
            // Set random quote
            if (quoteText != null && duneQuotes.Length > 0)
            {
                int quoteIndex = Random.Range(0, duneQuotes.Length);
                quoteText.text = $"\"{duneQuotes[quoteIndex]}\"";
                
                if (attributionText != null && quoteAttributions.Length > quoteIndex)
                {
                    attributionText.text = quoteAttributions[quoteIndex];
                }
            }
            
            StartCoroutine(SplashSequence());
        }
        
        private IEnumerator SplashSequence()
        {
            // Fade in
            if (fadeOverlay != null)
            {
                float elapsed = 0;
                Color color = fadeOverlay.color;
                color.a = 1f;
                fadeOverlay.color = color;
                
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / fadeInDuration;
                    color.a = 1f - t;
                    fadeOverlay.color = color;
                    yield return null;
                }
                
                color.a = 0f;
                fadeOverlay.color = color;
            }
            
            // Scale logo
            if (logoImage != null)
            {
                Vector3 startScale = Vector3.one * 0.5f;
                Vector3 targetScale = Vector3.one;
                logoImage.transform.localScale = startScale;
                
                float elapsed = 0;
                while (elapsed < logoScaleDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / logoScaleDuration;
                    logoImage.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    yield return null;
                }
            }
            
            // Wait for duration or tap
            float timer = 0;
            while (timer < splashDuration && !isSkipped)
            {
                timer += Time.deltaTime;
                
                // Check for tap to skip
                if (skipOnTap && (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
                {
                    isSkipped = true;
                }
                
                yield return null;
            }
            
            // Fade out
            if (fadeOverlay != null)
            {
                float elapsed = 0;
                Color color = fadeOverlay.color;
                color.a = 0f;
                fadeOverlay.color = color;
                
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / fadeOutDuration;
                    color.a = t;
                    fadeOverlay.color = color;
                    yield return null;
                }
                
                color.a = 1f;
                fadeOverlay.color = color;
            }
            
            // Load next scene
            LoadingScreenManager.Instance?.LoadScene(nextScene);
        }
        
        private void Update()
        {
            // Alternative skip with any key
            if (skipOnTap && Input.anyKeyDown && !Input.GetMouseButtonDown(0))
            {
                isSkipped = true;
            }
        }
        
        public void SetNextScene(string sceneName)
        {
            nextScene = sceneName;
        }
        
        public void Skip()
        {
            isSkipped = true;
        }
        
        // Dune quotes database
        private static string[] GetDuneQuotes()
        {
            return new string[]
            {
                "Fear is the mind-killer.",
                "The spice must flow.",
                "He who controls the spice controls the universe.",
                "Walk without rhythm, and you won't attract the worm.",
                "The sleeper must awaken.",
                "I must not fear. Fear is the mind-killer.",
                "Bless the Maker and His water.",
                "The mystery of life isn't a problem to solve, but a reality to experience.",
                "Without change, something sleeps inside us.",
                "The power to destroy a thing is the absolute control over it."
            };
        }
        
        private static string[] GetQuoteAttributions()
        {
            return new string[]
            {
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune, Book I",
                "— Dune Messiah",
                "— Dune, Book I"
            };
        }
    }
}
