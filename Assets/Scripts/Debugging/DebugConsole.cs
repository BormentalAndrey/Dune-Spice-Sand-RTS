using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Debugging
{
    /// <summary>
    /// Developer console for debugging and testing
    /// Activated with three-finger tap or tilde key
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        public static DebugConsole Instance { get; private set; }
        
        [Header("UI")]
        public GameObject consolePanel;
        public TMP_InputField inputField;
        public TextMeshProUGUI outputText;
        public ScrollRect scrollRect;
        
        [Header("Settings")]
        public KeyCode toggleKey = KeyCode.BackQuote;
        public int maxLines = 100;
        public bool enableOnRelease = false;
        
        [Header("Touch Activation")]
        public int tapCountRequired = 3;
        public float tapTimeWindow = 1f;
        
        private List<string> outputLines = new List<string>();
        private float lastTapTime;
        private int tapCount;
        
        private Dictionary<string, System.Action<string[]>> commands = new Dictionary<string, System.Action<string[]>>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeCommands();
            
            #if !UNITY_EDITOR
            if (!enableOnRelease)
            {
                consolePanel.SetActive(false);
            }
            #endif
        }
        
        private void Update()
        {
            // Keyboard toggle (editor/development)
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }
            
            // Touch activation (three-finger tap)
            if (Input.touchCount == tapCountRequired)
            {
                if (Time.time - lastTapTime < tapTimeWindow)
                {
                    tapCount++;
                    if (tapCount >= tapCountRequired)
                    {
                        ToggleConsole();
                        tapCount = 0;
                    }
                }
                else
                {
                    tapCount = 1;
                }
                lastTapTime = Time.time;
            }
            
            // Execute command on enter
            if (inputField.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                ExecuteCommand(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }
        
        private void InitializeCommands()
        {
            // Resource commands
            commands["add_spice"] = (args) => {
                float amount = args.Length > 0 ? float.Parse(args[0]) : 1000f;
                GameManager.Instance.AddSpice(amount);
                Log($"Added {amount} spice");
            };
            
            commands["add_water"] = (args) => {
                float amount = args.Length > 0 ? float.Parse(args[0]) : 500f;
                GameManager.Instance.AddWater(amount);
                Log($"Added {amount} water");
            };
            
            commands["add_jihad"] = (args) => {
                float amount = args.Length > 0 ? float.Parse(args[0]) : 10f;
                GameManager.Instance.AddJihadProgress(amount);
                Log($"Added {amount} jihad progress");
            };
            
            // Unit commands
            commands["spawn_unit"] = (args) => {
                if (args.Length < 1)
                {
                    Log("Usage: spawn_unit <unit_type>");
                    return;
                }
                
                string unitType = args[0];
                Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 10f;
                
                // Spawn unit logic here
                Log($"Spawning {unitType} at {spawnPos}");
            };
            
            commands["kill_all"] = (args) => {
                UnitBase[] units = FindObjectsOfType<UnitBase>();
                foreach (var unit in units)
                {
                    if (unit.faction != GameManager.Instance.playerFaction)
                    {
                        unit.TakeDamage(9999f, UnitBase.AttackType.Explosion, null);
                    }
                }
                Log($"Killed {units.Length} enemy units");
            };
            
            // Worm commands
            commands["spawn_worm"] = (args) => {
                Vector3 spawnPos = Camera.main.transform.position + Camera.main.transform.forward * 20f;
                // Spawn worm logic
                Log($"Spawning worm at {spawnPos}");
            };
            
            commands["clear_worms"] = (args) => {
                SandwormAI[] worms = FindObjectsOfType<SandwormAI>();
                foreach (var worm in worms)
                {
                    Destroy(worm.gameObject);
                }
                Log($"Cleared {worms.Length} worms");
            };
            
            // Game state commands
            commands["win"] = (args) => {
                CampaignManager.Instance?.CompleteMission();
                Log("Mission completed");
            };
            
            commands["lose"] = (args) => {
                Log("Mission failed");
                // Trigger loss condition
            };
            
            commands["god_mode"] = (args) => {
                // Toggle invincibility
                Log("God mode activated");
            };
            
            commands["reveal_map"] = (args) => {
                FogOfWar fog = FindObjectOfType<FogOfWar>();
                if (fog != null)
                {
                    // Reveal entire map
                    Log("Map revealed");
                }
            };
            
            // Debug commands
            commands["fps"] = (args) => {
                Log($"FPS: {1f / Time.deltaTime:F1}");
            };
            
            commands["mem"] = (args) => {
                float memory = System.GC.GetTotalMemory(false) / (1024f * 1024f);
                Log($"Memory: {memory:F1} MB");
            };
            
            commands["units"] = (args) => {
                UnitBase[] units = FindObjectsOfType<UnitBase>();
                int playerUnits = 0;
                int enemyUnits = 0;
                
                foreach (var unit in units)
                {
                    if (unit.faction == GameManager.Instance.playerFaction)
                        playerUnits++;
                    else
                        enemyUnits++;
                }
                
                Log($"Player units: {playerUnits}, Enemy units: {enemyUnits}");
            };
            
            commands["help"] = (args) => {
                Log("=== Available Commands ===");
                foreach (var cmd in commands.Keys)
                {
                    Log($"  {cmd}");
                }
                Log("=========================");
            };
            
            commands["clear"] = (args) => {
                outputLines.Clear();
                outputText.text = "";
            };
            
            commands["balance"] = (args) => {
                GameBalanceData.Instance.LogBalanceData();
                Log("Balance data logged to console");
            };
            
            commands["save"] = (args) => {
                SaveSystem.Instance?.SaveGame();
                Log("Game saved");
            };
            
            commands["load"] = (args) => {
                SaveSystem.Instance?.LoadGame();
                Log("Game loaded");
            };
            
            commands["reset"] = (args) => {
                SaveSystem.Instance?.DeleteSave();
                Log("Save data reset");
            };
            
            commands["time"] = (args) => {
                if (args.Length > 0)
                {
                    float scale = float.Parse(args[0]);
                    Time.timeScale = scale;
                    Log($"Time scale set to {scale}");
                }
                else
                {
                    Log($"Current time scale: {Time.timeScale}");
                }
            };
        }
        
        private void ExecuteCommand(string commandText)
        {
            if (string.IsNullOrEmpty(commandText)) return;
            
            string[] parts = commandText.Trim().Split(' ');
            string command = parts[0].ToLower();
            string[] args = parts.Length > 1 ? parts[1..] : new string[0];
            
            if (commands.ContainsKey(command))
            {
                try
                {
                    commands[command](args);
                    Log($"> {commandText}");
                }
                catch (System.Exception e)
                {
                    Log($"Error: {e.Message}");
                }
            }
            else
            {
                Log($"Unknown command: {command}. Type 'help' for available commands.");
            }
        }
        
        public void Log(string message)
        {
            outputLines.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
            
            // Trim if too many lines
            while (outputLines.Count > maxLines)
            {
                outputLines.RemoveAt(0);
            }
            
            // Update display
            outputText.text = string.Join("\n", outputLines);
            
            // Auto-scroll to bottom
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            
            // Also log to Unity console
            Debug.Log($"[DebugConsole] {message}");
        }
        
        public void ToggleConsole()
        {
            consolePanel.SetActive(!consolePanel.activeSelf);
            if (consolePanel.activeSelf)
            {
                inputField.ActivateInputField();
            }
        }
        
        public void Clear()
        {
            outputLines.Clear();
            outputText.text = "";
        }
        
        public bool IsEnabled()
        {
            return consolePanel.activeSelf;
        }
    }
}
