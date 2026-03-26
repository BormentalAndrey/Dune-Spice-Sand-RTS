using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.UI
{
    /// <summary>
    /// Unit selection and command panel
    /// References: Dune military command interface
    /// </summary>
    public class UnitSelectionPanel : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panel;
        public RectTransform panelRect;
        
        [Header("Unit Info")]
        public TextMeshProUGUI unitNameText;
        public TextMeshProUGUI unitHealthText;
        public Image healthBar;
        public Image shieldBar;
        
        [Header("Unit Stats")]
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI armorText;
        public TextMeshProUGUI speedText;
        
        [Header("Commands")]
        public Button moveButton;
        public Button attackButton;
        public Button stopButton;
        public Button abilityButton;
        
        [Header("Ability Icons")]
        public Image prescienceIcon;
        public Image voiceIcon;
        public Image wormRideIcon;
        public Image stealthIcon;
        
        private List<UnitBase> selectedUnits = new List<UnitBase>();
        private bool isMultiSelect = false;
        
        private void Start()
        {
            if (moveButton != null) moveButton.onClick.AddListener(OnMoveCommand);
            if (attackButton != null) attackButton.onClick.AddListener(OnAttackCommand);
            if (stopButton != null) stopButton.onClick.AddListener(OnStopCommand);
            if (abilityButton != null) abilityButton.onClick.AddListener(OnAbilityCommand);
            
            HidePanel();
        }
        
        public void ShowUnitInfo(UnitBase unit)
        {
            if (unit == null) return;
            
            selectedUnits.Clear();
            selectedUnits.Add(unit);
            isMultiSelect = false;
            
            UpdatePanelForUnit(unit);
            ShowPanel();
        }
        
        public void ShowUnitsInfo(List<UnitBase> units)
        {
            if (units == null || units.Count == 0)
            {
                HidePanel();
                return;
            }
            
            selectedUnits = units;
            isMultiSelect = units.Count > 1;
            
            if (isMultiSelect)
            {
                ShowMultiSelectInfo(units);
            }
            else
            {
                UpdatePanelForUnit(units[0]);
            }
            
            ShowPanel();
        }
        
        private void UpdatePanelForUnit(UnitBase unit)
        {
            if (unit == null || unit.unitData == null) return;
            
            // Basic info
            unitNameText.text = unit.unitName;
            
            // Health
            float healthPercent = unit.currentHealth / unit.maxHealth;
            healthBar.fillAmount = healthPercent;
            unitHealthText.text = $"{unit.currentHealth:F0}/{unit.maxHealth:F0}";
            
            // Shield
            if (shieldBar != null)
            {
                float shieldPercent = unit.shieldStrength / unit.maxShield;
                shieldBar.fillAmount = shieldPercent;
                shieldBar.gameObject.SetActive(unit.maxShield > 0);
            }
            
            // Stats
            damageText.text = $"DMG: {unit.damage}";
            armorText.text = $"ARM: {unit.armor}";
            speedText.text = $"SPD: {unit.speed:F1}";
            
            // Abilities
            UpdateAbilityIcons(unit);
        }
        
        private void ShowMultiSelectInfo(List<UnitBase> units)
        {
            // Calculate averages for multi-select
            float avgHealth = 0;
            float avgDamage = 0;
            float totalHealth = 0;
            float maxHealth = 0;
            
            foreach (var unit in units)
            {
                totalHealth += unit.currentHealth;
                maxHealth += unit.maxHealth;
                avgDamage += unit.damage;
            }
            
            avgHealth = totalHealth / units.Count;
            avgDamage /= units.Count;
            
            unitNameText.text = $"{units.Count} Units Selected";
            healthBar.fillAmount = totalHealth / maxHealth;
            unitHealthText.text = $"HP: {avgHealth:F0} avg";
            damageText.text = $"DMG: {avgDamage:F0} avg";
            armorText.text = $"ARM: {units[0].armor:F0} avg";
            speedText.text = $"SPD: {units[0].speed:F1} avg";
            
            // Hide shield for multi-select
            if (shieldBar != null) shieldBar.gameObject.SetActive(false);
        }
        
        private void UpdateAbilityIcons(UnitBase unit)
        {
            // Atreides prescience
            if (prescienceIcon != null)
            {
                prescienceIcon.gameObject.SetActive(unit is AtreidesUnit || unit is PaulUnit);
            }
            
            // Voice command
            if (voiceIcon != null)
            {
                voiceIcon.gameObject.SetActive(unit is BeneGesseritUnit);
            }
            
            // Worm riding
            if (wormRideIcon != null)
            {
                wormRideIcon.gameObject.SetActive(unit is FremenUnit);
            }
            
            // Stealth
            if (stealthIcon != null)
            {
                stealthIcon.gameObject.SetActive(unit is FremenUnit);
            }
        }
        
        private void ShowPanel()
        {
            if (panel != null) panel.SetActive(true);
        }
        
        private void HidePanel()
        {
            if (panel != null) panel.SetActive(false);
        }
        
        private void OnMoveCommand()
        {
            if (selectedUnits.Count == 0) return;
            
            // Enter move mode - wait for tap on ground
            TouchInputSystem inputSystem = FindObjectOfType<TouchInputSystem>();
            if (inputSystem != null)
            {
                // inputSystem.EnterMoveMode(selectedUnits);
            }
            
            HidePanel();
        }
        
        private void OnAttackCommand()
        {
            if (selectedUnits.Count == 0) return;
            
            // Enter attack mode - wait for tap on enemy
            TouchInputSystem inputSystem = FindObjectOfType<TouchInputSystem>();
            if (inputSystem != null)
            {
                // inputSystem.EnterAttackMode(selectedUnits);
            }
            
            HidePanel();
        }
        
        private void OnStopCommand()
        {
            foreach (var unit in selectedUnits)
            {
                if (unit != null && unit.navAgent != null)
                {
                    unit.navAgent.isStopped = true;
                    unit.currentState = UnitBase.UnitState.Idle;
                }
            }
            
            HidePanel();
        }
        
        private void OnAbilityCommand()
        {
            if (selectedUnits.Count != 1) return;
            
            UnitBase unit = selectedUnits[0];
            
            // Check unit type and activate appropriate ability
            if (unit is AtreidesUnit atreidesUnit)
            {
                // Show prescience UI
                UIManager.Instance?.ShowPrescienceUI();
            }
            else if (unit is FremenUnit fremenUnit)
            {
                // Show worm riding UI
                UIManager.Instance?.ShowWormRidingUI(fremenUnit);
            }
            else if (unit is BeneGesseritUnit bgUnit)
            {
                // Show voice command UI
                UIManager.Instance?.ShowVoiceUI();
            }
        }
        
        public void ClearSelection()
        {
            selectedUnits.Clear();
            HidePanel();
        }
    }
    
    // Placeholder unit types for ability detection
    public class AtreidesUnit : UnitBase { }
    public class PaulUnit : AtreidesUnit { }
    public class BeneGesseritUnit : UnitBase { }
}
