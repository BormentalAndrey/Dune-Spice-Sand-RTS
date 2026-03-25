using UnityEngine;
using System.Collections;

namespace Dune.SpiceAndSand.Ecology
{
    /// <summary>
    /// Procedural terrain generator for Arrakis desert
    /// References: Dune geography - sand dunes, rock formations, spice fields
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public int mapSize = 256;
        public int heightmapResolution = 129;
        public float terrainHeight = 20f;
        
        [Header("Sand Dunes")]
        public float duneFrequency = 0.05f;
        public float duneAmplitude = 5f;
        public float duneSteepness = 2f;
        
        [Header("Rock Formations - Dune, Book I")]
        public float rockFrequency = 0.02f;
        public float rockHeight = 15f;
        public float rockThreshold = 0.7f;
        
        [Header("Sietch Locations")]
        public int sietchCount = 5;
        public float sietchHeight = 10f;
        
        [Header("Spice Fields")]
        public int spiceFieldCount = 10;
        public float spiceFieldRadius = 5f;
        
        [Header("Water Table")]
        public float waterTableHeight = 0f;
        
        private Terrain terrain;
        private TerrainData terrainData;
        
        private void Start()
        {
            GenerateTerrain();
        }
        
        public void GenerateTerrain()
        {
            terrain = GetComponent<Terrain>();
            if (terrain == null)
                terrain = gameObject.AddComponent<Terrain>();
                
            terrainData = new TerrainData();
            terrainData.heightmapResolution = heightmapResolution;
            terrainData.size = new Vector3(mapSize, terrainHeight, mapSize);
            
            // Generate heightmap
            float[,] heights = new float[heightmapResolution, heightmapResolution];
            
            for (int x = 0; x < heightmapResolution; x++)
            {
                for (int z = 0; z < heightmapResolution; z++)
                {
                    float xPos = (float)x / heightmapResolution * mapSize;
                    float zPos = (float)z / heightmapResolution * mapSize;
                    
                    float height = GenerateHeight(xPos, zPos);
                    heights[x, z] = height;
                }
            }
            
            terrainData.SetHeights(0, 0, heights);
            
            // Set textures
            terrainData.terrainLayers = new TerrainLayer[2];
            terrainData.terrainLayers[0] = CreateSandLayer();
            terrainData.terrainLayers[1] = CreateRockLayer();
            
            terrain.terrainData = terrainData;
            
            // Generate features
            GenerateSietchLocations();
            GenerateSpiceFields();
        }
        
        private float GenerateHeight(float x, float z)
        {
            float height = 0f;
            
            // Base sand dunes
            float duneX = Mathf.Sin(x * duneFrequency) * Mathf.Cos(z * duneFrequency * 0.5f);
            float duneZ = Mathf.Cos(z * duneFrequency) * Mathf.Sin(x * duneFrequency * 0.7f);
            float duneHeight = (duneX + duneZ) * duneAmplitude;
            
            height += duneHeight;
            
            // Rock formations
            float rockNoise = Mathf.PerlinNoise(x * rockFrequency, z * rockFrequency);
            if (rockNoise > rockThreshold)
            {
                float rockHeightValue = Mathf.Pow((rockNoise - rockThreshold) / (1f - rockThreshold), 2f) * rockHeight;
                height += rockHeightValue;
            }
            
            // Add noise for realism
            float noise = Mathf.PerlinNoise(x * 0.01f, z * 0.01f) * 0.5f;
            height += noise;
            
            // Clamp to terrain height
            height = Mathf.Clamp(height / terrainHeight, 0f, 1f);
            
            return height;
        }
        
        private TerrainLayer CreateSandLayer()
        {
            TerrainLayer layer = new TerrainLayer();
            layer.name = "Sand";
            // TODO: Assign sand texture
            return layer;
        }
        
        private TerrainLayer CreateRockLayer()
        {
            TerrainLayer layer = new TerrainLayer();
            layer.name = "Rock";
            // TODO: Assign rock texture
            return layer;
        }
        
        private void GenerateSietchLocations()
        {
            for (int i = 0; i < sietchCount; i++)
            {
                float x = Random.Range(0f, mapSize);
                float z = Random.Range(0f, mapSize);
                
                // Find suitable location (rock formation)
                float height = terrainData.GetHeight(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
                
                if (height > sietchHeight)
                {
                    Vector3 position = new Vector3(x, height, z);
                    CreateSietchMarker(position);
                }
            }
        }
        
        private void CreateSietchMarker(Vector3 position)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(2f, 1f, 2f);
            marker.GetComponent<Renderer>().material.color = Color.yellow;
            marker.tag = "SietchLocation";
            
            // Add collider for detection
            BoxCollider collider = marker.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
        
        private void GenerateSpiceFields()
        {
            for (int i = 0; i < spiceFieldCount; i++)
            {
                float x = Random.Range(0f, mapSize);
                float z = Random.Range(0f, mapSize);
                float height = terrainData.GetHeight(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
                
                // Spice fields appear in sandy areas (not rock)
                if (height < rockHeight / terrainHeight)
                {
                    Vector3 position = new Vector3(x, height + 0.1f, z);
                    CreateSpiceField(position);
                }
            }
        }
        
        private void CreateSpiceField(Vector3 position)
        {
            // Create spice field object
            GameObject spiceField = new GameObject("SpiceField");
            spiceField.transform.position = position;
            spiceField.transform.localScale = Vector3.one * spiceFieldRadius;
            
            // Add spice field component
            SpiceField field = spiceField.AddComponent<SpiceField>();
            field.spiceAmount = Random.Range(500f, 1500f);
            field.maxSpice = field.spiceAmount;
            
            // Add visual
            MeshRenderer renderer = spiceField.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0.7f, 0.3f, 0.5f);
            
            MeshFilter filter = spiceField.AddComponent<MeshFilter>();
            filter.mesh = CreateSpiceFieldMesh();
        }
        
        private Mesh CreateSpiceFieldMesh()
        {
            Mesh mesh = new Mesh();
            
            // Create a flat circular mesh
            int segments = 32;
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            
            vertices[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        public Vector3 GetRandomSandPosition()
        {
            Vector3 position = Vector3.zero;
            int attempts = 0;
            
            while (attempts < 100)
            {
                float x = Random.Range(0f, mapSize);
                float z = Random.Range(0f, mapSize);
                float y = terrainData.GetHeight(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
                
                // Check if sand (not rock)
                if (y < rockHeight / terrainHeight)
                {
                    position = new Vector3(x, y, z);
                    break;
                }
                
                attempts++;
            }
            
            return position;
        }
        
        public float GetHeightAt(Vector3 position)
        {
            return terrainData.GetHeight(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        }
    }
}
