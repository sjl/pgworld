using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public abstract class Vegetation: MonoBehaviour {
	public float waterHeight;

	// Use this for initialization
	void Start() {
		
	}

	// Texturing
	public void RunTexturing() {
		// Get a reference to the terrain
		Terrain terrain = GetComponent<Terrain>();

		// Get a reference to the terrain data
		var terrainData = terrain.terrainData;

		var width = terrainData.alphamapWidth;
		var height = terrainData.alphamapHeight;
		var heightMap = terrainData.GetHeights(0, 0, width, height);

		// determain the mix of textures 1, 2, 3 and 4 to use
		var map = new float[width, height, 5];

		// Assign the new map to the terrainData
		terrainData.SetAlphamaps(0, 0, Texture(width, height, heightMap, map));
	}

	// Trees
	public void RunTreeGenerator() {
		// Get a reference to the terrain
		Terrain terrain = GetComponent<Terrain>();

		// Get a reference to the terrain data
		var terrainData = terrain.terrainData;

		var width = terrainData.alphamapWidth;
		var height = terrainData.alphamapHeight;
		var heightMap = terrainData.GetHeights(0, 0, width, height);

		// add the trees to the terrain
		AddTrees(width, height, heightMap, terrain);
	}

	public void RunRemoveTrees() {
		// Get a reference to the terrain
		Terrain terrain = GetComponent<Terrain>();

		var terrainData = terrain.terrainData;

		RemoveTrees(terrainData);
	}

	// Rocks
	public void RunRockGenerator() {
		// Get a reference to the terrain
		Terrain terrain = GetComponent<Terrain>();

		// Get a reference to the terrain data
		var terrainData = terrain.terrainData;

		var width = terrainData.alphamapWidth;
		var height = terrainData.alphamapHeight;
		var heightMap = terrainData.GetHeights(0, 0, width, height);

		// add rocks to the terrain
		AddRocks(width, height, heightMap, terrain);
	}

	public abstract float[,,] Texture(int width, int height, float[,] heightMap, float[,,] map);
	public abstract void AddTrees(int width, int height, float[,] heightMap, Terrain terrain);
	public abstract void RemoveTrees(TerrainData terrainData);
	public abstract void AddRocks(int width, int height, float[,] heightMap, Terrain terrain);

	// Update is called once per frame
	void Update() {
	
	}
}
