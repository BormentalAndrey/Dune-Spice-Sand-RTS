using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Dune.SpiceAndSand.Core;

namespace Dune.SpiceAndSand.Monetization
{
    /// <summary>
    /// In-app purchase manager for optional content
    /// References: Free-to-play model with cosmetic only purchases
    /// </summary>
    public class InAppPurchaseManager : MonoBehaviour
    {
        public static InAppPurchaseManager Instance { get; private set; }
        
        [Header("Products")]
        public List<Product> products = new List<Product>();
        
        [Header("Consumables")]
        public Product spicePackSmall;
        public Product spicePackLarge;
        public Product waterPack;
        
        [Header("Non-Consumables")]
        public Product removeAds;
        public Product fremenSkinPack;
        public Product atreidesSkinPack;
        public Product harkonnenSkinPack;
        
        [Header("Subscriptions")]
        public Product spicePass; // Monthly spice bonus
        
        [System.Serializable]
        public class Product
        {
            public string id;
            public string title;
            public string description;
            public float priceUSD;
            public ProductType type;
            public Dictionary<string, object> rewards;
        }
        
        public enum ProductType
        {
            Consumable,
            NonConsumable,
            Subscription
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeProducts();
        }
        
        private void InitializeProducts()
        {
            // Spice packs
            spicePackSmall = new Product
            {
                id = "com.dune.spicepack.small",
                title = "Small Spice Pack",
                description = "500 Spice - Fuel your war machine!",
                priceUSD = 0.99f,
                type = ProductType.Consumable,
                rewards = new Dictionary<string, object> { { "spice", 500f } }
            };
            
            spicePackLarge = new Product
            {
                id = "com.dune.spicepack.large",
                title = "Large Spice Pack",
                description = "2500 Spice - The spice must flow!",
                priceUSD = 3.99f,
                type = ProductType.Consumable,
                rewards = new Dictionary<string, object> { { "spice", 2500f } }
            };
            
            waterPack = new Product
            {
                id = "com.dune.waterpack",
                title = "Water Cache",
                description = "1000 Water - Precious water for the tribe",
                priceUSD = 1.99f,
                type = ProductType.Consumable,
                rewards = new Dictionary<string, object> { { "water", 1000f } }
            };
            
            // Cosmetic packs
            removeAds = new Product
            {
                id = "com.dune.removeads",
                title = "Remove Ads",
                description = "No more ads - Support the developers!",
                priceUSD = 4.99f,
                type = ProductType.NonConsumable
            };
            
            fremenSkinPack = new Product
            {
                id = "com.dune.skins.fremen",
                title = "Fremen Skin Pack",
                description = "Customize your Fremen units with unique desert patterns",
                priceUSD = 2.99f,
                type = ProductType.NonConsumable
            };
            
            atreidesSkinPack = new Product
            {
                id = "com.dune.skins.atreides",
                title = "Atreides Skin Pack",
                description = "Atreides heraldry and elite unit skins",
                priceUSD = 2.99f,
                type = ProductType.NonConsumable
            };
            
            harkonnenSkinPack = new Product
            {
                id = "com.dune.skins.harkonnen",
                title = "Harkonnen Skin Pack",
                description = "Menacing Harkonnen armor and weapon skins",
                priceUSD = 2.99f,
                type = ProductType.NonConsumable
            };
            
            // Subscription
            spicePass = new Product
            {
                id = "com.dune.spicepass",
                title = "Spice Pass",
                description = "500 Spice daily + exclusive skins",
                priceUSD = 9.99f,
                type = ProductType.Subscription,
                rewards = new Dictionary<string, object> { { "dailySpice", 500f } }
            };
            
            products.AddRange(new[] { spicePackSmall, spicePackLarge, waterPack, removeAds, 
                                      fremenSkinPack, atreidesSkinPack, harkonnenSkinPack, spicePass });
        }
        
        public void PurchaseProduct(Product product)
        {
            if (product == null) return;
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Use Google Play Billing
            StartCoroutine(PurchaseRoutine(product));
            #else
            // Editor simulation
            SimulatePurchase(product);
            #endif
        }
        
        private IEnumerator PurchaseRoutine(Product product)
        {
            // Simulate purchase flow
            yield return new WaitForSeconds(1f);
            
            // Process purchase
            ProcessPurchase(product);
        }
        
        private void SimulatePurchase(Product product)
        {
            Debug.Log($"Simulated purchase: {product.title} - ${product.priceUSD}");
            ProcessPurchase(product);
        }
        
        private void ProcessPurchase(Product product)
        {
            // Grant rewards
            if (product.rewards != null)
            {
                if (product.rewards.ContainsKey("spice"))
                {
                    float spiceAmount = Convert.ToSingle(product.rewards["spice"]);
                    GameManager.Instance.AddSpice(spiceAmount);
                    AchievementSystem.Instance?.UpdateAchievement("spice_purchased", (int)spiceAmount);
                }
                
                if (product.rewards.ContainsKey("water"))
                {
                    float waterAmount = Convert.ToSingle(product.rewards["water"]);
                    GameManager.Instance.AddWater(waterAmount);
                }
            }
            
            // Handle non-consumables
            if (product.type == ProductType.NonConsumable)
            {
                PlayerPrefs.SetInt(product.id, 1);
                PlayerPrefs.Save();
                
                if (product.id == removeAds.id)
                {
                    // Disable ads
                }
                else if (product.id.Contains("skins"))
                {
                    // Unlock skins
                }
            }
            
            // Handle subscription
            if (product.type == ProductType.Subscription)
            {
                StartCoroutine(SubscriptionRoutine(product));
            }
            
            // Show confirmation
            UIManager.Instance?.ShowNotification($"Purchased: {product.title}");
            
            // Analytics
            Debug.Log($"Purchase completed: {product.id}");
        }
        
        private IEnumerator SubscriptionRoutine(Product product)
        {
            while (true)
            {
                yield return new WaitForSeconds(86400f); // 24 hours
                
                // Grant daily rewards
                if (product.rewards.ContainsKey("dailySpice"))
                {
                    float dailySpice = Convert.ToSingle(product.rewards["dailySpice"]);
                    GameManager.Instance.AddSpice(dailySpice);
                    
                    // Show notification
                    UIManager.Instance?.ShowNotification($"Spice Pass: +{dailySpice} Spice");
                }
            }
        }
        
        public bool IsProductOwned(string productId)
        {
            return PlayerPrefs.GetInt(productId, 0) == 1;
        }
        
        public void RestorePurchases()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Query Google Play for owned items
            StartCoroutine(RestoreRoutine());
            #else
            Debug.Log("Restore purchases not available in editor");
            #endif
        }
        
        private IEnumerator RestoreRoutine()
        {
            yield return new WaitForSeconds(1f);
            
            // Simulate restore
            foreach (var product in products)
            {
                if (product.type == ProductType.NonConsumable && IsProductOwned(product.id))
                {
                    ProcessPurchase(product);
                }
            }
        }
        
        public string GetLocalizedPrice(Product product)
        {
            // Return formatted price for current region
            return $"${product.priceUSD:F2}";
        }
    }
}
