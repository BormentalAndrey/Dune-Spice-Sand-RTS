using System;
using UnityEngine;
using UnityEngine.AI;

namespace Dune.SpiceAndSand.Units
{
    /// <summary>
    /// Base class for all units - Book-accurate stats and behaviors
    /// References: Dune terminology, shield mechanics, worm attraction
    /// </summary>
    public abstract class UnitBase : MonoBehaviour
    {
        [Header("Unit Identity")]
        public UnitData unitData;
        public string unitName;
        public Faction faction;
        
        [Header("Combat Stats")]
        public float currentHealth;
        public float maxHealth;
        public float armor;
        public float shieldStrength;
        public float maxShield;
        public float damage;
        public AttackType attackType;
        
        [Header("Movement")]
        public float speed;
        public MovementType movementType;
        public NavMeshAgent navAgent;
        
        [Header("Shield Mechanics - Dune, Book I, Chapter 5")]
        public bool shieldActive = true;
        public float shieldRechargeRate = 2f;
        public float shieldRechargeDelay = 3f;
        private float lastDamageTime;
        
        [Header("Vibration Signature - Dune, Book I, Chapter 3")]
        public float vibrationIntensity = 0.5f;
        public bool isActiveHarvester = false;
        
        [Header("State")]
        public UnitState currentState = UnitState.Idle;
        public UnitBase currentTarget;
        
        protected virtual void Awake()
        {
            if (unitData != null)
            {
                InitializeFromData();
            }
            
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.speed = speed;
            }
        }
        
        protected virtual void InitializeFromData()
        {
            unitName = unitData.unitName;
            maxHealth = unitData.maxHealth;
            currentHealth = maxHealth;
            armor = unitData.armor;
            maxShield = unitData.maxShield;
            shieldStrength = maxShield;
            damage = unitData.damage;
            attackType = unitData.attackType;
            speed = unitData.speed;
            movementType = unitData.movementType;
            vibrationIntensity = unitData.vibrationIntensity;
        }
        
        protected virtual void Update()
        {
            UpdateShield();
            UpdateVibrationEmission();
            UpdateState();
        }
        
        private void UpdateShield()
        {
            if (!shieldActive) return;
            
            // Shield recharge after delay - Dune mechanics
            if (Time.time > lastDamageTime + shieldRechargeDelay)
            {
                shieldStrength = Mathf.Min(shieldStrength + shieldRechargeRate * Time.deltaTime, maxShield);
            }
        }
        
        private void UpdateVibrationEmission()
        {
            // Rhythmic vibrations attract sandworms
            // Dune, Book I, Chapter 3: "Walk without rhythm, and you won't attract the worm"
            if (vibrationIntensity > 0.1f && currentState != UnitState.Dead)
            {
                float effectiveVibration = isActiveHarvester ? vibrationIntensity * 3f : vibrationIntensity;
                WormManager.Instance?.RegisterVibration(effectiveVibration, transform.position);
            }
        }
        
        public virtual void TakeDamage(float amount, AttackType sourceType, UnitBase source)
        {
            // Shield mechanics - Dune, Book I, Chapter 5
            // Shields stop fast projectiles, allow slow blades
            bool isFastProjectile = (sourceType == AttackType.Lasgun || sourceType == AttackType.Projectile);
            
            if (shieldActive && shieldStrength > 0 && isFastProjectile)
            {
                // Lasgun + Shield = nuclear explosion - Dune, Book I, Chapter 15
                if (sourceType == AttackType.Lasgun)
                {
                    TriggerLasgunShieldExplosion();
                }
                
                float shieldDamage = Mathf.Min(amount, shieldStrength);
                shieldStrength -= shieldDamage;
                amount -= shieldDamage;
                
                if (shieldStrength <= 0)
                {
                    shieldActive = false;
                    OnShieldCollapse();
                }
            }
            
            if (amount > 0)
            {
                float mitigatedDamage = amount - armor;
                mitigatedDamage = Mathf.Max(mitigatedDamage, 1f);
                currentHealth -= mitigatedDamage;
                lastDamageTime = Time.time;
                
                if (currentHealth <= 0)
                {
                    Die();
                }
            }
        }
        
        private void TriggerLasgunShieldExplosion()
        {
            // Dune, Book I, Chapter 15: "A lasgun beam striking a shield creates a nuclear explosion"
            float explosionRadius = 15f;
            float explosionDamage = 500f;
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (var hitCollider in hitColliders)
            {
                UnitBase unit = hitCollider.GetComponent<UnitBase>();
                if (unit != null)
                {
                    unit.TakeDamage(explosionDamage, AttackType.Explosion, this);
                }
            }
            
            // Destroy this unit
            Die();
            
            // Visual effect
            // TODO: Add explosion VFX
        }
        
        protected virtual void OnShieldCollapse()
        {
            // Shield collapse effect
        }
        
        protected virtual void Die()
        {
            currentState = UnitState.Dead;
            
            // Remove vibration when dead
            // TODO: Add death animation
            Destroy(gameObject, 1f);
        }
        
        protected virtual void UpdateState()
        {
            // State machine logic
            if (currentState == UnitState.Dead) return;
            
            if (currentTarget != null && currentTarget.currentState != UnitState.Dead)
            {
                // Move to attack range
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
                float attackRange = GetAttackRange();
                
                if (distanceToTarget <= attackRange)
                {
                    currentState = UnitState.Attacking;
                    Attack(currentTarget);
                }
                else
                {
                    currentState = UnitState.Moving;
                    MoveTo(currentTarget.transform.position);
                }
            }
            else if (currentState == UnitState.Attacking)
            {
                currentState = UnitState.Idle;
            }
        }
        
        public virtual void Attack(UnitBase target)
        {
            // Override for specific attack behaviors
            target.TakeDamage(damage, attackType, this);
        }
        
        public void MoveTo(Vector3 position)
        {
            if (navAgent != null)
            {
                navAgent.SetDestination(position);
                currentState = UnitState.Moving;
            }
        }
        
        public void SetTarget(UnitBase target)
        {
            currentTarget = target;
        }
        
        protected virtual float GetAttackRange()
        {
            return 1.5f;
        }
        
        public enum UnitState
        {
            Idle,
            Moving,
            Attacking,
            Dead
        }
        
        public enum Faction
        {
            Atreides,
            Harkonnen,
            Fremen,
            Sardaukar,
            Neutral
        }
        
        public enum AttackType
        {
            MeleeBlade,     // Slow blade penetrates shields
            Projectile,     // Stopped by shields
            Lasgun,         // Causes shield explosion
            Poison,         // Harkonnen specialty
            Sonic,          // Atreides sonic weapon
            Explosion
        }
        
        public enum MovementType
        {
            Ground,     // Normal ground movement
            Sand,       // Moves on sand (Fremen bonus)
            Air,        // Flying units
            Worm        // Sandworm - special movement
        }
    }
}
