using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Notification system for game events and alerts
    /// References: Dune messenger system, Fremen warnings
    /// </summary>
    public class NotificationSystem : MonoBehaviour
    {
        public static NotificationSystem Instance { get; private set; }
        
        [Header("UI")]
        public GameObject notificationPrefab;
        public Transform notificationContainer;
        public int maxNotifications = 5;
        
        [Header("Settings")]
        public float notificationDuration = 4f;
        public float fadeOutTime = 0.5f;
        
        [Header("Colors")]
        public Color infoColor = Color.blue;
        public Color warningColor = Color.yellow;
        public Color dangerColor = Color.red;
        public Color successColor = Color.green;
        
        private Queue<Notification> notificationQueue = new Queue<Notification>();
        private List<GameObject> activeNotifications = new List<GameObject>();
        
        public enum NotificationType
        {
            Info,
            Warning,
            Danger,
            Success,
            Worm,
            Spice,
            Combat
        }
        
        private class Notification
        {
            public string message;
            public NotificationType type;
            public float duration;
            public string bookReference;
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            // Process queue if space available
            if (notificationQueue.Count > 0 && activeNotifications.Count < maxNotifications)
            {
                Notification notif = notificationQueue.Dequeue();
                ShowNotification(notif);
            }
        }
        
        private void ShowNotification(Notification notif)
        {
            GameObject notifObj = Instantiate(notificationPrefab, notificationContainer);
            activeNotifications.Add(notifObj);
            
            // Set text
            TextMeshProUGUI text = notifObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = notif.message;
            
            // Set color
            Image background = notifObj.GetComponent<Image>();
            if (background != null)
            {
                switch (notif.type)
                {
                    case NotificationType.Info:
                        background.color = infoColor;
                        break;
                    case NotificationType.Warning:
                        background.color = warningColor;
                        break;
                    case NotificationType.Danger:
                        background.color = dangerColor;
                        break;
                    case NotificationType.Success:
                        background.color = successColor;
                        break;
                    case NotificationType.Worm:
                        background.color = new Color(0.5f, 0.2f, 0f);
                        break;
                    case NotificationType.Spice:
                        background.color = new Color(1f, 0.7f, 0.2f);
                        break;
                    case NotificationType.Combat:
                        background.color = new Color(0.8f, 0.2f, 0.2f);
                        break;
                }
            }
            
            // Auto-destroy after duration
            StartCoroutine(DestroyNotification(notifObj, notif.duration));
        }
        
        private IEnumerator DestroyNotification(GameObject notifObj, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Fade out
            CanvasGroup canvasGroup = notifObj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float t = 0;
                while (t < fadeOutTime)
                {
                    t += Time.deltaTime;
                    canvasGroup.alpha = 1 - (t / fadeOutTime);
                    yield return null;
                }
            }
            
            activeNotifications.Remove(notifObj);
            Destroy(notifObj);
        }
        
        // Public notification methods
        public void ShowMessage(string message, NotificationType type = NotificationType.Info, float duration = -1f)
        {
            if (duration < 0) duration = notificationDuration;
            
            notificationQueue.Enqueue(new Notification
            {
                message = message,
                type = type,
                duration = duration
            });
        }
        
        public void ShowSpiceAlert(float amount, string location)
        {
            ShowMessage($"Spice bloom detected at {location}! +{amount} spice available.", 
                NotificationType.Spice, 3f);
        }
        
        public void ShowWormAlert(float distance)
        {
            if (distance < 20f)
            {
                ShowMessage("WORM APPROACHING! Evacuate harvesters immediately!", 
                    NotificationType.Danger, 5f);
            }
            else if (distance < 50f)
            {
                ShowMessage($"Worm detected {distance:F0}m away. Rhythmic vibrations attract it.", 
                    NotificationType.Warning, 4f);
            }
            else
            {
                ShowMessage("Distant worm sign detected. Be cautious.", 
                    NotificationType.Info, 3f);
            }
        }
        
        public void ShowHarvesterDestroyed()
        {
            ShowMessage("Spice harvester destroyed! The spice is lost.", 
                NotificationType.Danger, 4f);
        }
        
        public void ShowBaseUnderAttack()
        {
            ShowMessage("Your base is under attack! Defend the spice fields!", 
                NotificationType.Danger, 5f);
        }
        
        public void ShowUnitTrained(string unitName)
        {
            ShowMessage($"New {unitName} ready for deployment.", 
                NotificationType.Success, 2f);
        }
        
        public void ShowBuildingComplete(string buildingName)
        {
            ShowMessage($"{buildingName} construction complete.", 
                NotificationType.Success, 2f);
        }
        
        public void ShowAchievementUnlocked(string achievementName)
        {
            ShowMessage($"Achievement Unlocked: {achievementName}!", 
                NotificationType.Success, 5f);
        }
        
        public void ShowVoiceCommandSuccess(string command)
        {
            ShowMessage($"Voice command issued: {command}", 
                NotificationType.Info, 2f);
        }
        
        public void ShowPrescienceActivated()
        {
            ShowMessage("Prescience activated. Enemy movements revealed.", 
                NotificationType.Info, 3f);
        }
        
        public void ShowWaterLow()
        {
            ShowMessage("Water reserves critically low! Build windtraps immediately.", 
                NotificationType.Warning, 5f);
        }
        
        public void ShowJihadProgress(float progress)
        {
            if (progress >= 80f)
            {
                ShowMessage($"Jihad momentum: {progress:F0}% - The Fremen are rallying to your cause!", 
                    NotificationType.Warning, 4f);
            }
            else if (progress >= 50f)
            {
                ShowMessage($"Jihad momentum: {progress:F0}% - Fremen support growing.", 
                    NotificationType.Info, 3f);
            }
        }
        
        public void ClearAllNotifications()
        {
            foreach (var notif in activeNotifications)
            {
                Destroy(notif);
            }
            activeNotifications.Clear();
            notificationQueue.Clear();
        }
        
        // Dune-specific lore notifications
        public void ShowLisanAlGaibMessage()
        {
            ShowMessage("The Lisan al-Gaib! The Voice from the Outer World!", 
                NotificationType.Success, 5f);
        }
        
        public void ShowWaterOfLifeMessage()
        {
            ShowMessage("You have consumed the Water of Life. The past and future are one.", 
                NotificationType.Success, 6f);
        }
        
        public void ShowCrysknifeWarning()
        {
            ShowMessage("A crysknife drawn must not be sheathed without drawing blood.", 
                NotificationType.Warning, 4f);
        }
    }
}
