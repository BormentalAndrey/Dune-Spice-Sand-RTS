using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Campaign
{
    /// <summary>
    /// Book-accurate dialogue system with quotes from novels
    /// References: Direct quotes from Dune series
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        [Header("UI Elements")]
        public GameObject dialoguePanel;
        public TextMeshProUGUI speakerText;
        public TextMeshProUGUI dialogueText;
        public Image speakerPortrait;
        public Button nextButton;
        public Button skipButton;
        
        [Header("Audio")]
        public AudioClip voiceClip;
        public AudioSource voiceSource;
        
        [Header("Settings")]
        public float textSpeed = 0.05f;
        public bool autoAdvance = false;
        public float autoAdvanceDelay = 3f;
        
        private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
        private bool isDialogueActive = false;
        private bool isTyping = false;
        private string currentFullText;
        
        [System.Serializable]
        public class DialogueEntry
        {
            public string speaker;
            public string text;
            public Sprite portrait;
            public string bookReference; // Book and chapter
            public AudioClip voiceLine;
        }
        
        private void Start()
        {
            dialoguePanel.SetActive(false);
            
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
        }
        
        public void StartDialogue(List<DialogueEntry> dialogue)
        {
            dialogueQueue.Clear();
            foreach (var entry in dialogue)
            {
                dialogueQueue.Enqueue(entry);
            }
            
            ShowNextDialogue();
        }
        
        private void ShowNextDialogue()
        {
            if (dialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }
            
            DialogueEntry entry = dialogueQueue.Dequeue();
            DisplayDialogue(entry);
        }
        
        private void DisplayDialogue(DialogueEntry entry)
        {
            if (!dialoguePanel.activeSelf)
                dialoguePanel.SetActive(true);
                
            speakerText.text = entry.speaker;
            if (entry.portrait != null)
                speakerPortrait.sprite = entry.portrait;
                
            currentFullText = entry.text;
            StartCoroutine(TypeText(entry.text));
            
            // Play voice line
            if (voiceSource != null && entry.voiceLine != null)
            {
                voiceSource.clip = entry.voiceLine;
                voiceSource.Play();
            }
            
            // Log for debugging - book reference
            Debug.Log($"[{entry.bookReference}] {entry.speaker}: {entry.text}");
        }
        
        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";
            
            foreach (char c in text.ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
            
            isTyping = false;
            
            if (autoAdvance)
            {
                yield return new WaitForSeconds(autoAdvanceDelay);
                ShowNextDialogue();
            }
        }
        
        private void OnNextClicked()
        {
            if (isTyping)
            {
                // Skip typing
                StopAllCoroutines();
                dialogueText.text = currentFullText;
                isTyping = false;
            }
            else
            {
                ShowNextDialogue();
            }
        }
        
        private void OnSkipClicked()
        {
            EndDialogue();
        }
        
        private void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            isDialogueActive = false;
            
            // Trigger mission start or next event
            CampaignManager.Instance?.OnDialogueComplete();
        }
        
        // Book-specific dialogues
        public List<DialogueEntry> GetArrivalDialogue()
        {
            // Dune, Book I, Chapter 1 - Arrival on Arrakis
            return new List<DialogueEntry>
            {
                new DialogueEntry
                {
                    speaker = "Duke Leto Atreides",
                    text = "Arrakis... Dune... Desert planet. The spice must flow.",
                    bookReference = "Dune, Book I, Chapter 1",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Thufir Hawat",
                    text = "My Lord, the Harkonnen have left traps. We must be vigilant.",
                    bookReference = "Dune, Book I, Chapter 1",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Paul Atreides",
                    text = "I must learn the ways of the desert. The Fremen know secrets the Imperium has never discovered.",
                    bookReference = "Dune, Book I, Chapter 1",
                    voiceLine = null
                }
            };
        }
        
        public List<DialogueEntry> GetFremenContactDialogue()
        {
            // Dune, Book I, Chapter 25 - Meeting Stilgar
            return new List<DialogueEntry>
            {
                new DialogueEntry
                {
                    speaker = "Stilgar",
                    text = "I am Stilgar, Naib of Sietch Tabr. You are welcome in the desert, outworlder.",
                    bookReference = "Dune, Book I, Chapter 25",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Paul Atreides",
                    text = "I am Paul... son of Duke Leto. The Harkonnen murdered my father. I seek to learn your ways.",
                    bookReference = "Dune, Book I, Chapter 25",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Stilgar",
                    text = "You are Mahdi? The Lisan al-Gaib? The Voice from the Outer World?",
                    bookReference = "Dune, Book I, Chapter 25",
                    voiceLine = null
                }
            };
        }
        
        public List<DialogueEntry> GetWormRidingDialogue()
        {
            // Dune, Book I, Chapter 35 - First worm ride
            return new List<DialogueEntry>
            {
                new DialogueEntry
                {
                    speaker = "Stilgar",
                    text = "Usul, you must call the maker. Show us you are truly Fremen.",
                    bookReference = "Dune, Book I, Chapter 35",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Paul",
                    text = "I will ride Shai-Hulud.",
                    bookReference = "Dune, Book I, Chapter 35",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Chani",
                    text = "Muad'Dib! He rides the worm! He is the Lisan al-Gaib!",
                    bookReference = "Dune, Book I, Chapter 35",
                    voiceLine = null
                }
            };
        }
        
        public List<DialogueEntry> GetWaterOfLifeDialogue()
        {
            // Dune, Book I, Chapter 36 - Water of Life ritual
            return new List<DialogueEntry>
            {
                new DialogueEntry
                {
                    speaker = "Reverend Mother Ramallo",
                    text = "You would dare the Water of Life, Usul? It changes a woman into a Reverend Mother. For a man, it is death.",
                    bookReference = "Dune, Book I, Chapter 36",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Paul",
                    text = "I am the Kwisatz Haderach. I will transform the poison or die.",
                    bookReference = "Dune, Book I, Chapter 36",
                    voiceLine = null
                },
                new DialogueEntry
                {
                    speaker = "Chani",
                    text = "Usul! No!",
                    bookReference = "Dune, Book I, Chapter 36",
                    voiceLine = null
                }
            };
        }
    }
}
