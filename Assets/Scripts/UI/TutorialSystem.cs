using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Tutorial system for new players
    /// References: Dune lore and gameplay introduction
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        public static TutorialSystem Instance { get; private set; }
        
        [Header("UI")]
        public GameObject tutorialPanel;
        public TextMeshProUGUI tutorialText;
        public Image tutorialIcon;
        public Button nextButton;
        public Button skipButton;
        
        [Header("Highlight")]
        public GameObject highlightPrefab;
        public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);
        
        [Header("Settings")]
        public bool enableTutorial = true;
        public bool forceTutorialOnFirstPlay = true;
        
        private Queue<TutorialStep> tutorialSteps = new Queue<TutorialStep>();
        private TutorialStep currentStep;
        private GameObject currentHighlight;
        
        [System.Serializable]
        public class TutorialStep
        {
            public string title;
            public string message;
            public Sprite icon;
            public TutorialAction action;
            public string targetTag;
            public float autoAdvanceDelay = 0f;
            public string bookReference;
            
            public enum TutorialAction
            {
                WaitForTap,
                WaitForBuild,
                WaitForHarvest,
                WaitForCombat,
                WaitForWorm,
                WaitForVoice,
                AutoAdvance
            }
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Check if tutorial should be shown
            bool tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
            if (forceTutorialOnFirstPlay && !tutorialCompleted)
            {
                enableTutorial = true;
            }
        }
        
        private void Start()
        {
            if (enableTutorial)
            {
                InitializeTutorial();
            }
        }
        
        private void InitializeTutorial()
        {
            // Add tutorial steps
            AddStep(new TutorialStep
            {
                title = "Welcome to Arrakis",
                message = "You are House Atreides, newly appointed to rule the desert planet. The spice must flow!",
                action = TutorialStep.TutorialAction.AutoAdvance,
                autoAdvanceDelay = 3f,
                bookReference = "Dune, Book I, Chapter 1"
            });
            
            AddStep(new TutorialStep
            {
                title = "Select Your Units",
                message = "Tap on a unit to select it. Drag to select multiple units.",
                action = TutorialStep.TutorialAction.WaitForTap,
                targetTag = "Unit",
                bookReference = "Dune, Book I, Chapter 1"
            });
            
            AddStep(new TutorialStep
            {
                title = "Move Your Units",
                message = "Tap on the ground to move selected units. Explore the desert!",
                action = TutorialStep.TutorialAction.WaitForTap,
                targetTag = "Ground",
                bookReference = "Dune, Book I, Chapter 1"
            });
            
            AddStep(new TutorialStep
            {
                title = "Build a Spice Refinery",
                message = "Tap on an empty area, select 'Spice Refinery' from the build menu.",
                action = TutorialStep.TutorialAction.WaitForBuild,
                targetTag = "SpiceRefinery",
                bookReference = "Dune, Book I, Chapter 4"
            });
            
            AddStep(new TutorialStep
            {
                title = "Harvest Spice",
                message = "Your harvester will automatically collect spice. Watch for worms!",
                action = TutorialStep.TutorialAction.WaitForHarvest,
                targetTag = "Harvester",
                bookReference = "Dune, Book I, Chapter 3"
            });
            
            AddStep(new TutorialStep
            {
                title = "Beware the Worm",
                message = "Sandworms are attracted to rhythmic vibrations. Walk without rhythm!",
                action = TutorialStep.TutorialAction.WaitForWorm,
                targetTag = "Worm",
                bookReference = "Dune, Book I, Chapter 3"
            });
            
            AddStep(new TutorialStep
            {
                title = "Train Troops",
                message = "Build a Barracks to train more soldiers for your army.",
                action = TutorialStep.TutorialAction.WaitForBuild,
                targetTag = "Barracks",
                bookReference = "Dune, Book I, Chapter 8"
            });
            
            AddStep(new TutorialStep
            {
                title = "Congratulations!",
                message = "You now know the basics. May Shai-Hulud bless your journey!",
                action = TutorialStep.TutorialAction.AutoAdvance,
                autoAdvanceDelay = 4f,
                bookReference = "Dune, Book I, Chapter 35"
            });
            
            // Start tutorial
            StartCoroutine(StartTutorial());
        }
        
        private void AddStep(TutorialStep step)
        {
            tutorialSteps.Enqueue(step);
        }
        
        private IEnumerator StartTutorial()
        {
            yield return new WaitForSeconds(1f);
            
            while (tutorialSteps.Count > 0)
            {
                currentStep = tutorialSteps.Dequeue();
                yield return StartCoroutine(ShowTutorialStep(currentStep));
            }
            
            CompleteTutorial();
        }
        
        private IEnumerator ShowTutorialStep(TutorialStep step)
        {
            // Show panel
            tutorialPanel.SetActive(true);
            tutorialText.text = step.message;
            
            if (step.icon != null)
                tutorialIcon.sprite = step.icon;
            
            // Highlight target if needed
            if (!string.IsNullOrEmpty(step.targetTag))
            {
                HighlightTarget(step.targetTag);
            }
            
            // Handle action
            bool completed = false;
            
            switch (step.action)
            {
                case TutorialStep.TutorialAction.AutoAdvance:
                    yield return new WaitForSeconds(step.autoAdvanceDelay);
                    completed = true;
                    break;
                    
                case TutorialStep.TutorialAction.WaitForTap:
                    yield return StartCoroutine(WaitForTap(step.targetTag));
                    completed = true;
                    break;
                    
                case TutorialStep.TutorialAction.WaitForBuild:
                    yield return StartCoroutine(WaitForBuild(step.targetTag));
                    completed = true;
                    break;
                    
                case TutorialStep.TutorialAction.WaitForHarvest:
                    yield return StartCoroutine(WaitForHarvest());
                    completed = true;
                    break;
                    
                case TutorialStep.TutorialAction.WaitForCombat:
                    yield return StartCoroutine(WaitForCombat());
                    completed = true;
                    break;
                    
                case TutorialStep.TutorialAction.WaitForWorm:
                    yield return StartCoroutine(WaitForWorm());
                    completed = true;
                    break;
            }
            
            // Remove highlight
            if (currentHighlight != null)
            {
                Destroy(currentHighlight);
                currentHighlight = null;
            }
            
            tutorialPanel.SetActive(false);
        }
        
        private void HighlightTarget(string tag)
        {
            GameObject target = GameObject.FindGameObjectWithTag(tag);
            if (target != null && highlightPrefab != null)
            {
                currentHighlight = Instantiate(highlightPrefab, target.transform);
                currentHighlight.transform.localPosition = Vector3.zero;
                
                Renderer renderer = currentHighlight.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = highlightColor;
                }
            }
        }
        
        private IEnumerator WaitForTap(string tag)
        {
            bool tapped = false;
            
            while (!tapped)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (tag == "Ground" && hit.collider.CompareTag("Ground"))
                        {
                            tapped = true;
                        }
                        else if (hit.collider.CompareTag(tag))
                        {
                            tapped = true;
                        }
                    }
                }
                yield return null;
            }
        }
        
        private IEnumerator WaitForBuild(string buildingTag)
        {
            bool built = false;
            
            while (!built)
            {
                BuildingBase building = FindObjectOfType<BuildingBase>();
                if (building != null && building.CompareTag(buildingTag))
                {
                    built = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private IEnumerator WaitForHarvest()
        {
            bool harvested = false;
            float initialSpice = GameManager.Instance?.spice ?? 0;
            
            while (!harvested)
            {
                float currentSpice = GameManager.Instance?.spice ?? 0;
                if (currentSpice > initialSpice + 100f)
                {
                    harvested = true;
                }
                yield return new WaitForSeconds(1f);
            }
        }
        
        private IEnumerator WaitForCombat()
        {
            bool fought = false;
            
            while (!fought)
            {
                // Check if any unit has taken damage
                UnitBase[] units = FindObjectsOfType<UnitBase>();
                foreach (var unit in units)
                {
                    if (unit.currentHealth < unit.maxHealth)
                    {
                        fought = true;
                        break;
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private IEnumerator WaitForWorm()
        {
            bool wormSpawned = false;
            
            while (!wormSpawned)
            {
                SandwormAI worm = FindObjectOfType<SandwormAI>();
                if (worm != null)
                {
                    wormSpawned = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void CompleteTutorial()
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
            
            Debug.Log("Tutorial completed!");
            
            // Unlock achievement
            AchievementSystem.Instance?.UpdateAchievement("CgkI__tutorial_complete", 1);
        }
        
        public void SkipTutorial()
        {
            StopAllCoroutines();
            tutorialPanel.SetActive(false);
            CompleteTutorial();
        }
        
        public void RestartTutorial()
        {
            PlayerPrefs.SetInt("TutorialCompleted", 0);
            PlayerPrefs.Save();
            
            tutorialSteps.Clear();
            InitializeTutorial();
            StartCoroutine(StartTutorial());
        }
    }
}
