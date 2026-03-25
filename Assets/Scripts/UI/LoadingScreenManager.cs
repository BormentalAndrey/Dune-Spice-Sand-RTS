using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Loading screen with Dune quotes and lore
    /// References: Book quotes from Dune series
    /// </summary>
    public class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance { get; private set; }
        
        [Header("UI Elements")]
        public GameObject loadingPanel;
        public Slider progressBar;
        public TextMeshProUGUI loadingText;
        public TextMeshProUGUI quoteText;
        public TextMeshProUGUI tipText;
        
        [Header("Loading Quotes")]
        public string[] duneQuotes;
        public string[] gameplayTips;
        
        [Header("Settings")]
        public float minimumLoadTime = 2f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeQuotes();
        }
        
        private void InitializeQuotes()
        {
            // Dune book quotes
            duneQuotes = new string[]
            {
                "Fear is the mind-killer. - Dune, Book I",
                "The spice must flow. - Dune, Book I",
                "He who controls the spice controls the universe. - Dune, Book I",
                "Walk without rhythm, and you won't attract the worm. - Dune, Book I",
                "A beginning is the time for taking the most delicate care that the balances are correct. - Dune, Book I",
                "The sleeper must awaken. - Dune, Book I",
                "I must not fear. Fear is the mind-killer. - Dune, Book I",
                "Bless the Maker and His water. - Dune, Book I",
                "The mystery of life isn't a problem to solve, but a reality to experience. - Dune, Book I",
                "Without change, something sleeps inside us, and seldom awakens. - Dune Messiah",
                "The power to destroy a thing is the absolute control over it. - Dune, Book I",
                "There is no escape—we pay for the violence of our ancestors. - Children of Dune"
            };
            
            gameplayTips = new string[]
            {
                "Tip: Walk without rhythm to avoid attracting sandworms!",
                "Tip: Use thumpers to distract worms away from harvesters.",
                "Tip: Fremen can ride sandworms for fast transportation.",
                "Tip: Lasguns + Shields = Nuclear explosion. Use with caution!",
                "Tip: The Voice can temporarily control enemy units.",
                "Tip: Atreides prescience reveals enemy movements 5 seconds into the future.",
                "Tip: Build windtraps to collect water from the air.",
                "Tip: Fremen stillsuits recycle water from movement.",
                "Tip: Spice fields regenerate over time through the sandtrout cycle.",
                "Tip: Harkonnen poison deals damage over time to enemy units.",
                "Tip: Use ornithopters to scout the desert for spice fields.",
                "Tip: Carryalls can evacuate harvesters when worms approach."
            };
        }
        
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            loadingPanel.SetActive(true);
            
            // Display random quote and tip
            if (quoteText != null)
                quoteText.text = "\"" + duneQuotes[Random.Range(0, duneQuotes.Length)] + "\"";
                
            if (tipText != null)
                tipText.text = gameplayTips[Random.Range(0, gameplayTips.Length)];
            
            // Start async load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < minimumLoadTime || asyncLoad.progress < 0.9f)
            {
                elapsedTime += Time.deltaTime;
                
                // Calculate progress
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
                float displayProgress = Mathf.Max(progress, timeProgress);
                
                if (progressBar != null)
                    progressBar.value = displayProgress;
                    
                if (loadingText != null)
                    loadingText.text = $"Loading... {Mathf.RoundToInt(displayProgress * 100)}%";
                
                // Update loading tip every 2 seconds
                if (elapsedTime % 2f < Time.deltaTime)
                {
                    if (tipText != null)
                        tipText.text = gameplayTips[Random.Range(0, gameplayTips.Length)];
                }
                
                yield return null;
            }
            
            // Activate scene
            asyncLoad.allowSceneActivation = true;
            
            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Hide loading screen after scene starts
            yield return new WaitForSeconds(0.5f);
            loadingPanel.SetActive(false);
        }
        
        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            loadingPanel.SetActive(true);
            
            // Display random quote
            if (quoteText != null)
                quoteText.text = "\"" + duneQuotes[Random.Range(0, duneQuotes.Length)] + "\"";
                
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            asyncLoad.allowSceneActivation = false;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < minimumLoadTime || asyncLoad.progress < 0.9f)
            {
                elapsedTime += Time.deltaTime;
                
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
                float displayProgress = Mathf.Max(progress, timeProgress);
                
                if (progressBar != null)
                    progressBar.value = displayProgress;
                    
                if (loadingText != null)
                    loadingText.text = $"Loading... {Mathf.RoundToInt(displayProgress * 100)}%";
                
                yield return null;
            }
            
            asyncLoad.allowSceneActivation = true;
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            yield return new WaitForSeconds(0.5f);
            loadingPanel.SetActive(false);
        }
        
        public void ShowLoading(bool show)
        {
            loadingPanel.SetActive(show);
        }
    }
}
