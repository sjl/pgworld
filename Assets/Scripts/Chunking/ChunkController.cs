using UnityEngine;
using System;
using System.Collections;
using CielaSpike;

struct ChunkCoord {
  public readonly int x;
  public readonly int z;
  public ChunkCoord(int chunkx, int chunkz) {
    x = chunkx;
    z = chunkz;
  }
}

struct RenderedHeightmap {
	public readonly ChunkCoord coord;
	public readonly float[,] heightmap;
	public RenderedHeightmap(ChunkCoord c, float[,] h) {
		coord = c;
		heightmap = h;
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

	public Texture2D texture;
	public Texture2D normal;

	private Hashtable chunks;
	private Queue heightmapQueue;

	private int chunkResolution;
	private float chunkWidth;

	private PerlinNoise noise;

	void Start () {
		noise = new PerlinNoise();
		noise.baseOctave = noiseBaseOctave;
		noise.weights = noiseWeights;
		noise.Init();

		chunkResolution = (int)Math.Pow(2, chunkExponent) + 1;
		chunkWidth = chunkResolution * chunkScale;

		chunks = new Hashtable();
		heightmapQueue = Queue.Synchronized(new Queue());
	}

	float[,] noiseHeightmap(ChunkCoord c, float[,] heightmap) {
		int ox = c.x * chunkResolution - c.x;
		int oz = c.z * chunkResolution - c.z;

		int width = chunkResolution;
		int height = chunkResolution;

		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				heightmap[z, x] = noise.Get(ox + x, oz + z);
			}
		}

		return heightmap;
	}

	IEnumerator GenerateHeightmap(ChunkCoord c) {
		print("Generating heightmap for coord:");
		print(c.x);
		print(c.z);

		// This is called on a background thread.  It needs to build the
		// heightmap array (and possibly some other stuff) and push it onto the
		// queue when ready.

		float[,] heightmap = new float[chunkResolution, chunkResolution];

		noiseHeightmap(c, heightmap);

		// Done, push it into the queue so the main thread can process it into
		// a terrain object.
		heightmapQueue.Enqueue(new RenderedHeightmap(c, heightmap));

		yield return Ninja.JumpToUnity;
	}

	void drainHeightmapQueue() {
		// Called on the main thread to drain one item per frame from the
		// heightmap queue.
		//
		// Tread carefully, here be concurrency.
		if (heightmapQueue.Count > 0) {
			finalizeHeightmap((RenderedHeightmap)heightmapQueue.Dequeue());
		}
	}

	void finalizeHeightmap(RenderedHeightmap rh) {
		TerrainData tData = new TerrainData();

		tData.heightmapResolution = chunkResolution;
		tData.size = new Vector3(chunkWidth, terrainHeight, chunkWidth);

		/* Create and position the terrain */
		GameObject terrain = Terrain.CreateTerrainGameObject(tData);
		terrain.transform.position = new Vector3(
				chunkWidth * rh.coord.x,
				0.0f,
				chunkWidth * rh.coord.z);

		/* Set the heightmap data from the background thread. */
		tData.SetHeights(0, 0, rh.heightmap);

		stitchTerrain(tData, rh.coord);
		textureTerrain(tData);

		chunks.Add(rh.coord, tData);
	}

	void ensureChunk(ChunkCoord c) {
		if (!chunks.Contains(c)) {
			chunks.Add(c, null);
			this.StartCoroutineAsync(GenerateHeightmap(c));
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

	void textureTerrain(TerrainData tData) {
		SplatPrototype[] splats = new SplatPrototype[1];

		splats[0] = new SplatPrototype();
		splats[0].texture = texture;
		splats[0].normalMap = normal;
		splats[0].tileSize = new Vector2(5, 5);

		tData.splatPrototypes = splats;
	}

	void Update () {
		drainHeightmapQueue();

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
