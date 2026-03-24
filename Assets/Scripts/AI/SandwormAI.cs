using UnityEngine;
using UnityEngine.AI;
using Dune.SpiceAndSand.Core;
using Dune.SpiceAndSand.Units;

namespace Dune.SpiceAndSand.Ecology
{
    /// <summary>
    /// Sandworm AI - Shai-Hulud, Old Man of the Desert
    /// References: Dune, Book I, Chapter 3, 35, 36
    /// </summary>
    public class SandwormAI : MonoBehaviour
    {
        [Header("Worm Stats")]
        public float health = 1000f;
        public float damage = 150f;
        public float speed = 12f;
        public float attackRadius = 5f;
        public float detectionRadius = 30f;
        
        [Header("Movement")]
        private NavMeshAgent navAgent;
        private Vector3 currentTarget;
        private bool hasTarget = false;
        
        [Header("Worm Riding - Dune, Book I, Chapter 35")]
        public bool isRidden = false;
        public UnitBase rider;
        
        [Header("State")]
        public WormState currentState = WormState.Patrolling;
        
        public System.Action OnDestroyed;
        
        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.speed = speed;
                navAgent.avoidancePriority = 0; // Highest priority
            }
            
            // Start patrolling
            StartCoroutine(PatrolRoutine());
        }
        
        private void Update()
        {
            if (isRidden) return; // Controlled by rider
            
            if (hasTarget && navAgent != null)
            {
                navAgent.SetDestination(currentTarget);
            }
            
            // Check for nearby vibration sources
            DetectVibrations();
        }
        
        private void DetectVibrations()
        {
            // Find closest vibration source
            // This is handled by WormManager - worms are drawn to highest vibration
        }
        
        public void SetTargetPosition(Vector3 position)
        {
            currentTarget = position;
            hasTarget = true;
            currentState = WormState.MovingToTarget;
        }
        
        private System.Collections.IEnumerator PatrolRoutine()
        {
            while (currentState == WormState.Patrolling)
            {
                // Random patrol movement
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * detectionRadius;
                randomPoint.y = transform.position.y;
                
                SetTargetPosition(randomPoint);
                yield return new WaitForSeconds(5f);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isRidden) return;
            
            UnitBase unit = other.GetComponent<UnitBase>();
            if (unit != null && unit.currentState != UnitBase.UnitState.Dead)
            {
                // Eat unit
                unit.TakeDamage(damage * 10f, UnitBase.AttackType.MeleeBlade, null);
                
                // Attract to vibration - Dune, Book I, Chapter 3
                WormManager.Instance?.RegisterVibration(5f, transform.position);
            }
            
            BuildingBase building = other.GetComponent<BuildingBase>();
            if (building != null)
            {
                building.TakeDamage(damage);
            }
        }
        
        public void TakeDamage(float amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
        }
        
        private void Die()
        {
            // Water of Life - Dune, Book I, Chapter 36
            // Drowning a worm produces the Water of Life
            if (isRidden)
            {
                // Rider gets the Water of Life
                GameManager.Instance.AddWater(500f);
                // TODO: Trigger Water of Life ritual
            }
            
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
        
        public void Mount(UnitBase riderUnit)
        {
            // Fremen worm riding - Dune, Book I, Chapter 35
            if (riderUnit is FremenUnit)
            {
                isRidden = true;
                rider = riderUnit;
                riderUnit.transform.SetParent(transform);
                riderUnit.gameObject.SetActive(false); // Rider is on worm
                currentState = WormState.Ridden;
            }
        }
        
        public void Dismount()
        {
            if (rider != null)
            {
                rider.transform.SetParent(null);
                rider.gameObject.SetActive(true);
                rider.transform.position = transform.position + Vector3.up * 2f;
                rider = null;
                isRidden = false;
                currentState = WormState.Patrolling;
            }
        }
        
        public enum WormState
        {
            Patrolling,
            MovingToTarget,
            Attacking,
            Ridden,
            Dead
        }
    }
}
