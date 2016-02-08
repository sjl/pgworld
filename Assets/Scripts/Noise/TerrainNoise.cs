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

	private void NormalizeHeightmap(float[,] heightmap, int width, int height) {
		float y;
		float min = float.MaxValue, max = float.MinValue;

		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				y = heightmap[z, x];
				if (y > max) max = y;
				if (y < min) min = y;
			}
		}

		float scale = 1 / (max - min);

		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				y = heightmap[z, x];
				heightmap[z, x] = (y - min) * scale;
			}
		}
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

		NormalizeHeightmap(heightmap, width, height);
		terrain.terrainData.SetHeights(0, 0, heightmap);
	}

}
