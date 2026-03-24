using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Units
{
    /// <summary>
    /// Atreides prescience ability - Paul's genetic memory
    /// References: Dune, Book II, "The Prescient Vision"
    /// </summary>
    public class AtreidesPrescience : MonoBehaviour
    {
        [Header("Prescience Settings")]
        public float prescienceRadius = 20f;
        public float prescienceDuration = 5f;
        public float prescienceCooldown = 30f;
        public float spiceCostPerUse = 50f;
        
        [Header("Future Prediction")]
        public bool isPrescienceActive = false;
        public List<Vector3> predictedEnemyPositions = new List<Vector3>();
        public float predictionTime = 5f; // See 5 seconds into future
        
        [Header("Fog of War")]
        public Material prescienceMaterial;
        public LayerMask enemyLayer;
        
        private float nextUseTime = 0f;
        private Camera mainCamera;
        private GameObject prescienceIndicator;
        
        private void Start()
        {
            mainCamera = Camera.main;
        }
        
        public bool ActivatePrescience(Vector3 centerPosition)
        {
            // Check if enough spice - Dune: Melange enhances prescience
            if (Time.time < nextUseTime) return false;
            if (!GameManager.Instance.ConsumeSpice(spiceCostPerUse)) return false;
            
            StartCoroutine(PrescienceRoutine(centerPosition));
            nextUseTime = Time.time + prescienceCooldown;
            return true;
        }
        
        private IEnumerator PrescienceRoutine(Vector3 centerPosition)
        {
            isPrescienceActive = true;
            
            // Reveal fog of war
            RevealArea(centerPosition);
            
            // Predict enemy movements
            PredictEnemyMovements(centerPosition);
            
            // Visual effect - Spice trance
            ShowPrescienceVisuals(centerPosition);
            
            yield return new WaitForSeconds(prescienceDuration);
            
            isPrescienceActive = false;
            HidePrescienceVisuals();
        }
        
        private void RevealArea(Vector3 center)
        {
            // Temporarily reveal fog of war in radius
            // TODO: Implement fog of war system
            
            Collider[] enemies = Physics.OverlapSphere(center, prescienceRadius, enemyLayer);
            foreach (var enemy in enemies)
            {
                UnitBase unit = enemy.GetComponent<UnitBase>();
                if (unit != null)
                {
                    // Mark as revealed
                    unit.gameObject.layer = LayerMask.NameToLayer("Revealed");
                    StartCoroutine(HideAfterDelay(unit, prescienceDuration));
                }
            }
        }
        
        private IEnumerator HideAfterDelay(UnitBase unit, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (unit != null)
            {
                unit.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
        
        private void PredictEnemyMovements(Vector3 center)
        {
            // Calculate predicted positions based on current velocities
            predictedEnemyPositions.Clear();
            
            Collider[] enemies = Physics.OverlapSphere(center, prescienceRadius, enemyLayer);
            foreach (var enemy in enemies)
            {
                UnitBase unit = enemy.GetComponent<UnitBase>();
                if (unit != null && unit.currentState == UnitBase.UnitState.Moving)
                {
                    // Predict future position
                    Vector3 velocity = unit.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero;
                    Vector3 predictedPos = unit.transform.position + velocity * predictionTime;
                    predictedEnemyPositions.Add(predictedPos);
                    
                    // Show prediction indicator
                    ShowPredictionIndicator(predictedPos);
                }
            }
        }
        
        private void ShowPredictionIndicator(Vector3 position)
        {
            // Spawn temporary marker at predicted position
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.position = position;
            indicator.transform.localScale = Vector3.one * 0.5f;
            indicator.GetComponent<Renderer>().material = prescienceMaterial;
            Destroy(indicator, prescienceDuration);
        }
        
        private void ShowPrescienceVisuals(Vector3 center)
        {
            // Visual effect - Spice-induced prescience
            if (prescienceIndicator == null)
            {
                prescienceIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                prescienceIndicator.transform.localScale = Vector3.one * prescienceRadius * 2f;
                prescienceIndicator.GetComponent<Renderer>().material = prescienceMaterial;
                prescienceIndicator.transform.position = center;
            }
            
            // Camera effect
            if (mainCamera != null)
            {
                StartCoroutine(CameraEffect());
            }
        }
        
        private IEnumerator CameraEffect()
        {
            float originalFOV = mainCamera.fieldOfView;
            float targetFOV = originalFOV * 1.2f;
            
            float t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                mainCamera.fieldOfView = Mathf.Lerp(originalFOV, targetFOV, t / 0.5f);
                yield return null;
            }
            
            yield return new WaitForSeconds(prescienceDuration);
            
            t = 0;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                mainCamera.fieldOfView = Mathf.Lerp(targetFOV, originalFOV, t / 0.5f);
                yield return null;
            }
        }
        
        private void HidePrescienceVisuals()
        {
            if (prescienceIndicator != null)
                Destroy(prescienceIndicator);
        }
        
        // Dune Messiah reference: Prescience trap
        public bool CheckForPrescienceTrap()
        {
            // Paul's prescience could be trapped by the Guild
            // Returns true if using prescience reveals a trap
            float trapChance = GameManager.Instance.jihadMeter / 100f;
            return Random.value < trapChance;
        }
    }
}
