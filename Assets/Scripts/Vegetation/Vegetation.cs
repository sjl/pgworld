using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public abstract class Vegetation: MonoBehaviour {
	TerrainData terrainData;
	Terrain terrain;

	// Use this for initialization
	void Start () {
		
	}

	// Texturing
	public void RunTexturing() {
		// Get a reference to the terrain
		terrain = GetComponent<Terrain> ();

		// Get a reference to the terrain data
		terrainData = terrain.terrainData;

		var width = terrainData.alphamapWidth;
		var height = terrainData.alphamapHeight;
		var heightMap = terrainData.GetHeights (0, 0, width, height);

		// determain the mix of textures 1, 2, 3 and 4 to use
		var map = new float[width, height, 4];

		// Assign the new map to the terrainData
		terrainData.SetAlphamaps(0, 0, Textured(width, height, heightMap, map));
	}

	public abstract float[,,] Textured(int width, int height, float[,] heightMap, float[,,] map);
	
	// Update is called once per frame
	void Update () {
	
	}
}
