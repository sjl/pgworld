using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public abstract class TerrainErosion : MonoBehaviour {
	Terrain terrain;

	void Start() {
		terrain = GetComponent<Terrain>();
	}

	public void Run() {
		int width = terrain.terrainData.heightmapWidth;
		int height = terrain.terrainData.heightmapHeight;
		float[,] heightmap = terrain.terrainData.GetHeights(0, 0, width, height);
		terrain.terrainData.SetHeights(0, 0, Erode(heightmap, width, height));
	}

	public abstract float[,] Erode(float[,] heightmap, int width, int height);
}
