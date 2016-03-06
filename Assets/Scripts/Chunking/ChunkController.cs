using UnityEngine;
using System;
using System.Collections;

struct ChunkCoord {
  public readonly int x;
  public readonly int z;
  public ChunkCoord(int chunkx, int chunkz) {
    x = chunkx;
    z = chunkz;
  }
}

public class ChunkController : MonoBehaviour {

	public GameObject player;
	public int chunkExponent = 6;
	public int chunkHorizon = 3;
	public float chunkScale = 1;
	public float terrainHeight = 20.0f;

	public float noiseBaseOctave = 0.25f;
	public float[] noiseWeights = {1.0f, 1.0f, 1.0f, 1.0f, 1.0f};

	private Hashtable chunks;
	private int chunkResolution;
	private float chunkWidth;

	private PerlinNoise noise;

	void Start () {
		noise = new PerlinNoise();
		noise.Init();

		chunks = new Hashtable();
		chunkResolution = (int)Math.Pow(2, chunkExponent) + 1;
		chunkWidth = chunkResolution * chunkScale;
	}

	void ensureChunk(ChunkCoord c) {
		if (!chunks.Contains(c)) {
			chunks.Add(c, makeChunk(c));
		}
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

	void stitchTerrain(TerrainData tData, ChunkCoord c) {
		TerrainData left   = (TerrainData)chunks[new ChunkCoord(c.x - 1, c.z)];
		TerrainData right  = (TerrainData)chunks[new ChunkCoord(c.x + 1, c.z)];
		TerrainData top    = (TerrainData)chunks[new ChunkCoord(c.x,     c.z - 1)];
		TerrainData bottom = (TerrainData)chunks[new ChunkCoord(c.x,     c.z + 1)];

		if (left != null) {
			var seam = left.GetHeights(chunkResolution - 1, 0, 1, chunkResolution);
			tData.SetHeights(0, 0, seam);
		}
		if (right != null) {
			var seam = right.GetHeights(0, 0, 1, chunkResolution);
			tData.SetHeights(chunkResolution - 1, 0, seam);
		}
		if (top != null) {
			var seam = top.GetHeights(0, chunkResolution-1, chunkResolution, 1);
			tData.SetHeights(0, 0, seam);
		}
		if (bottom != null) {
			var seam = bottom.GetHeights(0, 0, chunkResolution, 1);
			tData.SetHeights(0, chunkResolution - 1, seam);
		}
	}

	void noiseTerrain(TerrainData tData, ChunkCoord c) {
		int ox = c.x * chunkResolution - c.x;
		int oz = c.z * chunkResolution - c.z;

		int width = tData.heightmapWidth;
		int height = tData.heightmapHeight;

		float[,] heightmap = new float[height, width];
		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				heightmap[z, x] = noise.Get(ox + x, oz + z);
			}
		}

		NormalizeHeightmap(heightmap, width, height);
		tData.SetHeights(0, 0, heightmap);
		stitchTerrain(tData, c);
	}

	TerrainData makeChunk(ChunkCoord c) {
		TerrainData tData = new TerrainData();

		tData.heightmapResolution = chunkResolution;
		tData.size = new Vector3(chunkWidth, terrainHeight, chunkWidth);

		/* Create and position the terrain */
		GameObject terrain = Terrain.CreateTerrainGameObject(tData);
		terrain.transform.position = new Vector3(
				chunkWidth * c.x,
				0.0f,
				chunkWidth * c.z);

		noiseTerrain(tData, c);

		return tData;
	}

	void Update () {
		float px = player.transform.position.x;
		float pz = player.transform.position.z;

		int cx = (int) (px / chunkWidth);
		int cz = (int) (pz / chunkWidth);

		for (int dx = -chunkHorizon; dx <= chunkHorizon; dx++) {
			for (int dz = -chunkHorizon; dz <= chunkHorizon; dz++) {
				ensureChunk(new ChunkCoord(cx + dx, cz + dz));
			}
		}
	}
}
