using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(Noise))]
public class TerrainNoise : MonoBehaviour {
	Terrain terrain;
	Noise noise;

	void Start() {
		terrain = GetComponent<Terrain>();
		noise = GetComponent<Noise>();
	}

	public void Run() {
		noise.Init();

		int width = terrain.terrainData.heightmapWidth;
		int height = terrain.terrainData.heightmapHeight;

		float[,] heightmap = new float[height, width];
		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				heightmap[z, x] = noise.Get(x, z);
			}
		}

		terrain.terrainData.SetHeights(0, 0, heightmap);
	}

}
