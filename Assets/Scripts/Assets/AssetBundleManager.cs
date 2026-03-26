using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dune.SpiceAndSand.Assets
{
    /// <summary>
    /// Asset bundle management for efficient loading and memory usage
    /// References: Mobile optimization for Dune assets
    /// </summary>
    public class AssetBundleManager : MonoBehaviour
    {
        public static AssetBundleManager Instance { get; private set; }
        
        [Header("Asset Bundle Settings")]
        public string assetBundleUrl = "https://dunespiceandsand.com/bundles/";
        public bool useLocalBundles = true;
        public bool loadFromStreamingAssets = true;
        
        [Header("Caching")]
        public int maxCacheSize = 500 * 1024 * 1024; // 500 MB
        public uint cacheExpirationDays = 7;
        
        [Header("Loading")]
        public float loadingTimeout = 30f;
        public int maxConcurrentLoads = 3;
        
        private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
        private Dictionary<string, Object> loadedAssets = new Dictionary<string, Object>();
        private Queue<BundleLoadRequest> loadQueue = new Queue<BundleLoadRequest>();
        private int activeLoads = 0;
        
        public class BundleLoadRequest
        {
            public string bundleName;
            public System.Action<AssetBundle> onComplete;
            public System.Action<string> onError;
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
            
            InitializeCaching();
        }
        
        private void InitializeCaching()
        {
            if (Caching.ready)
            {
                Caching.maximumAvailableDiskSpace = maxCacheSize;
                Caching.expirationDelay = cacheExpirationDays;
            }
        }
        
        private void Update()
        {
            // Process load queue
            if (loadQueue.Count > 0 && activeLoads < maxConcurrentLoads)
            {
                BundleLoadRequest request = loadQueue.Dequeue();
                StartCoroutine(LoadBundleCoroutine(request));
                activeLoads++;
            }
        }
        
        public void LoadBundle(string bundleName, System.Action<AssetBundle> onComplete, System.Action<string> onError = null)
        {
            // Check if already loaded
            if (loadedBundles.ContainsKey(bundleName))
            {
                onComplete?.Invoke(loadedBundles[bundleName]);
                return;
            }
            
            // Queue load
            loadQueue.Enqueue(new BundleLoadRequest
            {
                bundleName = bundleName,
                onComplete = onComplete,
                onError = onError
            });
        }
        
        private IEnumerator LoadBundleCoroutine(BundleLoadRequest request)
        {
            string bundlePath = GetBundlePath(request.bundleName);
            UnityWebRequest requestWeb = UnityWebRequestAssetBundle.GetAssetBundle(bundlePath);
            
            requestWeb.timeout = (int)loadingTimeout;
            yield return requestWeb.SendWebRequest();
            
            if (requestWeb.result == UnityWebRequest.Result.Success)
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(requestWeb);
                loadedBundles[request.bundleName] = bundle;
                request.onComplete?.Invoke(bundle);
            }
            else
            {
                Debug.LogError($"Failed to load bundle {request.bundleName}: {requestWeb.error}");
                request.onError?.Invoke(requestWeb.error);
            }
            
            activeLoads--;
        }
        
        private string GetBundlePath(string bundleName)
        {
            if (useLocalBundles && loadFromStreamingAssets)
            {
                return Path.Combine(Application.streamingAssetsPath, "Bundles", bundleName);
            }
            else if (useLocalBundles)
            {
                return Path.Combine(Application.persistentDataPath, "Bundles", bundleName);
            }
            else
            {
                return assetBundleUrl + bundleName;
            }
        }
        
        public T LoadAsset<T>(string bundleName, string assetName) where T : Object
        {
            string key = $"{bundleName}_{assetName}";
            
            // Check cache
            if (loadedAssets.ContainsKey(key))
            {
                return loadedAssets[key] as T;
            }
            
            // Load from bundle
            if (loadedBundles.ContainsKey(bundleName))
            {
                T asset = loadedBundles[bundleName].LoadAsset<T>(assetName);
                loadedAssets[key] = asset;
                return asset;
            }
            
            Debug.LogWarning($"Bundle {bundleName} not loaded yet");
            return null;
        }
        
        public IEnumerator LoadAssetAsync<T>(string bundleName, string assetName, System.Action<T> onComplete) where T : Object
        {
            string key = $"{bundleName}_{assetName}";
            
            // Check cache
            if (loadedAssets.ContainsKey(key))
            {
                onComplete?.Invoke(loadedAssets[key] as T);
                yield break;
            }
            
            // Ensure bundle is loaded
            bool bundleLoaded = false;
            LoadBundle(bundleName, bundle =>
            {
                bundleLoaded = true;
            }, error =>
            {
                Debug.LogError($"Failed to load bundle for asset: {error}");
            });
            
            while (!bundleLoaded)
            {
                yield return null;
            }
            
            // Load asset
            if (loadedBundles.ContainsKey(bundleName))
            {
                AssetBundleRequest request = loadedBundles[bundleName].LoadAssetAsync<T>(assetName);
                yield return request;
                
                T asset = request.asset as T;
                loadedAssets[key] = asset;
                onComplete?.Invoke(asset);
            }
        }
        
        public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
            if (loadedBundles.ContainsKey(bundleName))
            {
                loadedBundles[bundleName].Unload(unloadAllLoadedObjects);
                loadedBundles.Remove(bundleName);
                
                // Clear cached assets from this bundle
                List<string> toRemove = new List<string>();
                foreach (var kvp in loadedAssets)
                {
                    if (kvp.Key.StartsWith(bundleName))
                    {
                        toRemove.Add(kvp.Key);
                    }
                }
                foreach (string key in toRemove)
                {
                    loadedAssets.Remove(key);
                }
            }
        }
        
        public void UnloadAllBundles()
        {
            foreach (var bundle in loadedBundles.Values)
            {
                bundle.Unload(false);
            }
            loadedBundles.Clear();
            loadedAssets.Clear();
            
            // Force garbage collection
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        
        public void DownloadBundlesManifest(System.Action<Dictionary<string, long>> onComplete)
        {
            StartCoroutine(DownloadManifestCoroutine(onComplete));
        }
        
        private IEnumerator DownloadManifestCoroutine(System.Action<Dictionary<string, long>> onComplete)
        {
            string manifestPath = GetBundlePath("manifest.json");
            UnityWebRequest request = UnityWebRequest.Get(manifestPath);
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse manifest
                Dictionary<string, long> bundleSizes = new Dictionary<string, long>();
                // TODO: Parse JSON manifest
                onComplete?.Invoke(bundleSizes);
            }
            else
            {
                Debug.LogError("Failed to download manifest");
                onComplete?.Invoke(null);
            }
        }
        
        private void OnDestroy()
        {
            UnloadAllBundles();
        }
    }
}
