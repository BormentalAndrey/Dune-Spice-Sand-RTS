using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Abilities
{
    /// <summary>
    /// The Voice - Bene Gesserit ability
    /// References: Dune, Book I, Chapter 1, 5, 20
    /// </summary>
    public class VoiceCommandAbility : MonoBehaviour
    {
        [Header("Voice Settings")]
        public float voiceRange = 10f;
        public float voiceCooldown = 15f;
        public float commandDuration = 5f;
        public float spiceCostPerUse = 30f;
        
        [Header("Command Types")]
        public enum VoiceCommand
        {
            Stop,           // "Stop!"
            Attack,         // "Kill!"
            Retreat,        // "Flee!"
            Reveal,         // "Speak!" - Reveal hidden enemies
            Surrender       // "Yield!" - Temporarily disable
        }
        
        [Header("UI")]
        public GameObject voiceCommandPanel;
        public float voiceRecognitionDelay = 2f;
        
        private float nextUseTime = 0f;
        private List<UnitBase> affectedUnits = new List<UnitBase>();
        
        // Mobile voice command (button combo alternative)
        public bool UseVoiceCommand(Vector3 position, VoiceCommand command)
        {
            if (Time.time < nextUseTime) return false;
            if (!GameManager.Instance.ConsumeSpice(spiceCostPerUse)) return false;
            
            // Find units in range
            Collider[] colliders = Physics.OverlapSphere(position, voiceRange);
            List<UnitBase> targets = new List<UnitBase>();
            
            foreach (var collider in colliders)
            {
                UnitBase unit = collider.GetComponent<UnitBase>();
                if (unit != null && unit.faction != GameManager.Instance.playerFaction)
                {
                    targets.Add(unit);
                }
            }
            
            if (targets.Count == 0) return false;
            
            // Apply command
            foreach (var target in targets)
            {
                ApplyCommand(target, command);
            }
            
            nextUseTime = Time.time + voiceCooldown;
            
            // Visual effect - Voice wave
            ShowVoiceWave(position);
            
            return true;
        }
        
        private void ApplyCommand(UnitBase unit, VoiceCommand command)
        {
            switch (command)
            {
                case VoiceCommand.Stop:
                    unit.currentState = UnitBase.UnitState.Idle;
                    if (unit.navAgent != null)
                        unit.navAgent.isStopped = true;
                    StartCoroutine(ReleaseCommand(unit, commandDuration));
                    break;
                    
                case VoiceCommand.Attack:
                    // Find nearest ally to attack
                    Collider[] allies = Physics.OverlapSphere(unit.transform.position, 20f);
                    foreach (var ally in allies)
                    {
                        UnitBase allyUnit = ally.GetComponent<UnitBase>();
                        if (allyUnit != null && allyUnit.faction == unit.faction)
                        {
                            unit.SetTarget(allyUnit);
                            break;
                        }
                    }
                    StartCoroutine(ReleaseCommand(unit, commandDuration));
                    break;
                    
                case VoiceCommand.Retreat:
                    // Move away from player units
                    Vector3 retreatDir = (unit.transform.position - transform.position).normalized;
                    Vector3 retreatPos = unit.transform.position + retreatDir * 20f;
                    unit.MoveTo(retreatPos);
                    StartCoroutine(ReleaseCommand(unit, commandDuration));
                    break;
                    
                case VoiceCommand.Reveal:
                    // Reveal hidden units (Fremen stealth)
                    unit.GetComponent<FremenUnit>()?.DeactivateStealth();
                    break;
                    
                case VoiceCommand.Surrender:
                    unit.currentState = UnitBase.UnitState.Idle;
                    StartCoroutine(SurrenderEffect(unit));
                    break;
            }
            
            affectedUnits.Add(unit);
        }
        
        private IEnumerator ReleaseCommand(UnitBase unit, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (unit != null && unit.currentState != UnitBase.UnitState.Dead)
            {
                unit.currentState = UnitBase.UnitState.Idle;
                if (unit.navAgent != null)
                    unit.navAgent.isStopped = false;
            }
            affectedUnits.Remove(unit);
        }
        
        private IEnumerator SurrenderEffect(UnitBase unit)
        {
            // Disable unit temporarily
            unit.enabled = false;
            yield return new WaitForSeconds(commandDuration);
            if (unit != null)
                unit.enabled = true;
        }
        
        private void ShowVoiceWave(Vector3 position)
        {
            // Create expanding ring effect
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.transform.position = position;
            ring.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
            ring.GetComponent<Renderer>().material.color = Color.magenta;
            Destroy(ring, 0.5f);
            
            // Expand ring
            StartCoroutine(ExpandRing(ring, voiceRange));
        }
        
        private IEnumerator ExpandRing(GameObject ring, float maxRadius)
        {
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float radius = Mathf.Lerp(0.1f, maxRadius, t / 0.5f);
                ring.transform.localScale = new Vector3(radius, 0.01f, radius);
                yield return null;
            }
        }
        
        // Mobile alternative: Button combo voice system
        public void ShowVoiceMenu()
        {
            voiceCommandPanel.SetActive(true);
            // Show buttons for different commands
        }
        
        public void OnVoiceCommandSelected(int commandIndex)
        {
            VoiceCommand command = (VoiceCommand)commandIndex;
            // Get touch position from input system
            Vector3 touchPos = Input.mousePosition; // Placeholder for mobile
            Ray ray = Camera.main.ScreenPointToRay(touchPos);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                UseVoiceCommand(hit.point, command);
            }
            
            voiceCommandPanel.SetActive(false);
        }
    }
}
