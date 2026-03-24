using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dune.SpiceAndSand.Ecology
{
    /// <summary>
    /// Sand terrain with spice blooms and worm activity
    /// References: Dune ecology, spice cycle from Appendix
    /// </summary>
    public class SandTerrain : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public Terrain terrain;
        public float sandHeight = 0f;
        public float rockHeight = 10f;
        
        [Header("Spice Fields")]
        public List<SpiceField> spiceFields = new List<SpiceField>();
        public GameObject spiceFieldPrefab;
        public int initialSpiceFields = 5;
        
        [Header("Sandworm Tracks")]
        public GameObject wormTrackPrefab;
        public float trackDecayTime = 30f;
        
        [Header("Sandstorms - Dune, Book I, Chapter 16")]
        public bool isSandstormActive = false;
        public float sandstormDuration = 60f;
        public float sandstormCooldown = 300f;
        public float sandstormSpeed = 10f;
        public Vector3 sandstormDirection;
        
        [Header("Water Table - Terraforming")]
        public float waterTableDepth = 100f;
        public float terraformingProgress = 0f;
        
        private List<GameObject> activeTracks = new List<GameObject>();
        
        private void Start()
        {
            InitializeTerrain();
            StartCoroutine(SandstormCycle());
        }
        
        private void InitializeTerrain()
        {
            // Generate spice fields
            for (int i = 0; i < initialSpiceFields; i++)
            {
                Vector3 position = GetRandomSandPosition();
                SpawnSpiceField(position);
            }
        }
        
        public void SpawnSpiceField(Vector3 position)
        {
            GameObject fieldObj = Instantiate(spiceFieldPrefab, position, Quaternion.identity);
            SpiceField field = fieldObj.GetComponent<SpiceField>();
            spiceFields.Add(field);
        }
        
        public Vector3 GetRandomSandPosition()
        {
            // Get random point on terrain that is sand
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            
            // Check if sand (height below rock level)
            if (y < rockHeight)
            {
                return new Vector3(x, y, z);
            }
            
            // Retry if on rock
            return GetRandomSandPosition();
        }
        
        public void CreateWormTracks(Vector3 startPos, Vector3 endPos)
        {
            // Create visual worm tracks
            GameObject tracks = Instantiate(wormTrackPrefab, startPos, Quaternion.identity);
            activeTracks.Add(tracks);
            
            // Orient towards end position
            Vector3 direction = endPos - startPos;
            tracks.transform.rotation = Quaternion.LookRotation(direction);
            tracks.transform.localScale = new Vector3(1, 1, direction.magnitude);
            
            StartCoroutine(DecayTracks(tracks));
        }
        
        private IEnumerator DecayTracks(GameObject tracks)
        {
            yield return new WaitForSeconds(trackDecayTime);
            
            // Fade out
            Renderer renderer = tracks.GetComponent<Renderer>();
            float t = 0;
            while (t < 2f)
            {
                t += Time.deltaTime;
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(1, 0, t / 2f);
                renderer.material.color = color;
                yield return null;
            }
            
            activeTracks.Remove(tracks);
            Destroy(tracks);
        }
        
        private IEnumerator SandstormCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(sandstormCooldown - 60f, sandstormCooldown + 60f));
                
                // Start sandstorm
                isSandstormActive = true;
                sandstormDirection = Random.insideUnitSphere;
                sandstormDirection.y = 0;
                sandstormDirection.Normalize();
                
                // Notify all windtraps
                Windtrap[] windtraps = FindObjectsOfType<Windtrap>();
                foreach (var windtrap in windtraps)
                {
                    windtrap.OnSandstormEnter();
                }
                
                // Visual effect - Sandstorm particles
                StartCoroutine(SandstormVisuals());
                
                yield return new WaitForSeconds(sandstormDuration);
                
                isSandstormActive = false;
                foreach (var windtrap in windtraps)
                {
                    windtrap.OnSandstormExit();
                }
            }
        }
        
        private IEnumerator SandstormVisuals()
        {
            // TODO: Add particle system for sandstorm
            // Reduce visibility, add wind force
            yield return null;
        }
        
        public void AdvanceTerraforming(float amount)
        {
            // Fremen terraforming goal - Dune, Book I, Chapter 35
            terraformingProgress += amount;
            
            if (terraformingProgress >= 100f)
            {
                CompleteTerraforming();
            }
        }
        
        private void CompleteTerraforming()
        {
            // Transform desert to green - Dune Messiah / Children of Dune
            // Change terrain textures, add water, reduce spice blooms
            
            // Lower sand height (water rising)
            sandHeight -= 5f;
            rockHeight -= 3f;
            
            // TODO: Update terrain data
        }
    }
    
    /// <summary>
    /// Spice field - Harvestable resource
    /// References: Dune, Book I, Chapter 3
    /// </summary>
    public class SpiceField : MonoBehaviour
    {
        [Header("Spice Field")]
        public float spiceAmount = 1000f;
        public float maxSpice = 1000f;
        public float regrowthRate = 0.1f; // Spice per second after sandtrout cycle
        public bool isDepleted = false;
        
        [Header("Visual")]
        public ParticleSystem spiceParticles;
        public Light spiceGlow;
        
        private float regrowthTimer = 0f;
        
        private void Update()
        {
            if (isDepleted)
            {
                regrowthTimer += Time.deltaTime;
                if (regrowthTimer >= 60f) // 1 minute regrowth delay
                {
                    // Spice regrowth - Dune ecology: Sandtrout/Spice cycle
                    spiceAmount = Mathf.Min(spiceAmount + regrowthRate, maxSpice);
                    if (spiceAmount >= maxSpice * 0.1f)
                    {
                        isDepleted = false;
                        regrowthTimer = 0f;
                        ReactivateField();
                    }
                }
            }
        }
        
        public float ExtractSpice(float amount)
        {
            float extracted = Mathf.Min(amount, spiceAmount);
            spiceAmount -= extracted;
            
            if (spiceAmount <= 0)
            {
                DepleteField();
            }
            
            // Visual feedback
            if (spiceParticles != null)
                spiceParticles.Emit((int)extracted);
                
            return extracted;
        }
        
        private void DepleteField()
        {
            isDepleted = true;
            spiceAmount = 0;
            
            if (spiceGlow != null)
                spiceGlow.intensity = 0;
                
            if (spiceParticles != null)
                spiceParticles.Stop();
        }
        
        private void ReactivateField()
        {
            if (spiceGlow != null)
                spiceGlow.intensity = 1f;
                
            if (spiceParticles != null)
                spiceParticles.Play();
        }
        
        public bool HasSpice => !isDepleted && spiceAmount > 0;
    }
}
