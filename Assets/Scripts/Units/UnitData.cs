using UnityEngine;

namespace Dune.SpiceAndSand.Units
{
    /// <summary>
    /// ScriptableObject for unit data - Book-accurate stats
    /// </summary>
    [CreateAssetMenu(fileName = "New Unit", menuName = "Dune/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("Identity")]
        public string unitName;
        public string loreQuote; // Book citation
        
        [Header("Combat Stats")]
        public float maxHealth = 100f;
        public float armor = 10f;
        public float maxShield = 0f;
        public float damage = 25f;
        public UnitBase.AttackType attackType;
        
        [Header("Movement")]
        public float speed = 5f;
        public UnitBase.MovementType movementType;
        
        [Header("Cost - Spice & Water")]
        public float spiceCost = 100f;
        public float waterCost = 10f;
        
        [Header("Special")]
        public float vibrationIntensity = 0.5f;
        public bool canRideWorm = false; // Fremen only
        public bool hasPrescience = false; // Atreides only
        
        [Header("Book Reference")]
        public string bookReference; // e.g., "Dune, Book I, Chapter 8"
        
        // Atreides Trooper - Dune, Book I, Chapter 1
        public static UnitData AtreidesTrooper()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Atreides Trooper";
            data.loreQuote = "The Atreides troops are loyal to the death.";
            data.maxHealth = 120f;
            data.armor = 15f;
            data.maxShield = 50f;
            data.damage = 20f;
            data.attackType = UnitBase.AttackType.Projectile;
            data.speed = 4.5f;
            data.spiceCost = 80f;
            data.waterCost = 5f;
            data.bookReference = "Dune, Book I, Chapter 1";
            return data;
        }
        
        // Fremen Fedaykin - Dune, Book II, Chapter 20
        public static UnitData FremenFedaykin()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Fedaykin Commando";
            data.loreQuote = "Fedaykin - the death commandos of Muad'Dib.";
            data.maxHealth = 150f;
            data.armor = 20f;
            data.maxShield = 0f; // Fremen don't use shields in desert
            data.damage = 35f;
            data.attackType = UnitBase.AttackType.MeleeBlade;
            data.speed = 6f;
            data.spiceCost = 150f;
            data.waterCost = 0f; // Fremen use stillsuits
            data.vibrationIntensity = 0.2f; // Silent movement
            data.canRideWorm = true;
            data.bookReference = "Dune, Book II, Chapter 20";
            return data;
        }
        
        // Sandworm - Dune, Book I, Chapter 3
        public static UnitData Sandworm()
        {
            UnitData data = CreateInstance<UnitData>();
            data.unitName = "Shai-Hulud";
            data.loreQuote = "Bless the Maker and His water.";
            data.maxHealth = 1000f;
            data.armor = 50f;
            data.maxShield = 0f;
            data.damage = 150f;
            data.attackType = UnitBase.AttackType.MeleeBlade;
            data.speed = 12f;
            data.movementType = UnitBase.MovementType.Worm;
            data.vibrationIntensity = 0f; // They create vibration, not attracted
            data.bookReference = "Dune, Book I, Chapter 3";
            return data;
        }
    }
}
