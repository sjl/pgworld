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
	public readonly float[,,] splatmap;
	public RenderedHeightmap(ChunkCoord c, float[,] h, float[,,] sm) {
		coord = c;
		heightmap = h;
		splatmap = sm;
	}
}

public class ChunkController : MonoBehaviour {
	public GameObject player;
	public int chunkExponent = 6;
	public int chunkHorizon = 3;
	public int fringeSize = 1;
	public float chunkScale = 1;
	public float terrainHeight = 20.0f;

	public float noiseBaseOctave = 0.25f;
	public float[] noiseWeights = {
		1.0f, 1.0f, 1.0f, 1.0f, 1.0f
	};

	public Texture2D[] diffuses;
	public Texture2D[] normals;
	private SplatPrototype[] splats;
	public float slopeValue;
	public float mountainPeekHeight;
	public float waterHeight;

	public int reverseErosionIterations = 10;
	public float reverseErosionTalus = 0.001f;
	public float reverseErosionStrength = 1.0f;
	public float reverseErosionReverseTalusCutoff = 0.0001f;

	public int thermalErosionIterations = 10;
	public float thermalErosionTalus = 0.001f;
	public float thermalErosionStrength = 1.0f;


	private Hashtable chunks;
	private Queue heightmapQueue;

	private int chunkResolution;
	private float chunkWidth;

	private PerlinNoise noise;
	private ThermalTerrainErosion reverseThermalEroder;
	private ThermalTerrainErosion thermalEroder;
	private Texturer texturer;

	void Start () {
		noise = new PerlinNoise();
		noise.baseOctave = noiseBaseOctave;
		noise.weights = noiseWeights;
		noise.Init();

		chunkResolution = (int)Math.Pow(2, chunkExponent) + 1;
		chunkWidth = chunkResolution * chunkScale;

		chunks = new Hashtable();
		heightmapQueue = Queue.Synchronized(new Queue());

		reverseThermalEroder = new ThermalTerrainErosion();
		reverseThermalEroder.talus = this.reverseErosionTalus;
		reverseThermalEroder.strength = this.reverseErosionStrength;
		reverseThermalEroder.reverse = true;
		reverseThermalEroder.reverseTalusCutoff = this.reverseErosionReverseTalusCutoff ;
		reverseThermalEroder.iterations = this.reverseErosionIterations;

		thermalEroder = new ThermalTerrainErosion();
		thermalEroder.talus = this.thermalErosionTalus;
		thermalEroder.strength = this.thermalErosionStrength;
		thermalEroder.reverse = false;
		thermalEroder.iterations = this.thermalErosionIterations;

		texturer = new Texturer();
		texturer.slopeValue = slopeValue;
		texturer.mountainPeekHeight = mountainPeekHeight;
		texturer.waterHeight = waterHeight;
		splats = new SplatPrototype[diffuses.Length];

		for (var i = 0; i < diffuses.Length; i++) {
			splats[i] = new SplatPrototype();
			splats[i].texture = diffuses[i];
			splats[i].normalMap = normals[i];
			splats[i].tileSize = new Vector2(5, 5);
		}

		ensureChunk(new ChunkCoord(0, 0), false);
	}

	void noiseHeightmap(ChunkCoord c, float[,] heightmap) {
		int ox = c.x * chunkResolution - c.x - fringeSize;
		int oz = c.z * chunkResolution - c.z - fringeSize;

		int width = heightmap.GetLength(0);
		int height = heightmap.GetLength(1);

		for (int z = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				heightmap[z, x] = noise.Get(ox + x, oz + z);
			}
		}
	}

	void erodeHeightmap(ChunkCoord c, float[,] heightmap) {
		reverseThermalEroder.Erode(heightmap);
		thermalEroder.Erode(heightmap);
	}

	float[,,] textureHeightmap(float[,] heightmap) {
		// determain the mix of textures 1, 2, 3 and 4 to use
		var splatmap = new float[heightmap.GetLength(0), heightmap.GetLength(1), diffuses.Length];

		splatmap = texturer.Texture(heightmap, splatmap);

		return splatmap;
		// tData.splatPrototypes = splats;
	}

	void generateHeightmapSync(ChunkCoord c) {
		int heightmapResolution = chunkResolution + (2 * fringeSize);
		float[,] heightmap = new float[heightmapResolution, heightmapResolution];

		noiseHeightmap(c, heightmap);
		erodeHeightmap(c, heightmap);

		heightmap = extractChunk(heightmap);

		var splatmap = textureHeightmap(heightmap);

		// Done, push it into the queue so the main thread can process it into
		// a terrain object.
		heightmapQueue.Enqueue(new RenderedHeightmap(c, heightmap, splatmap));
	}

	IEnumerator generateHeightmapAsync(ChunkCoord c) {
		// This is called on a background thread.  It needs to build the
		// heightmap array (and possibly some other stuff) and push it onto the
		// queue when ready.

		generateHeightmapSync(c);

		yield return Ninja.JumpToUnity;
	}

	void drainHeightmapQueue(bool fully) {
		// Called on the main thread to drain finished heightmaps from the
		// heightmap queue.
		//
		// Tread carefully, here be concurrency.
		if (fully) {
			while (heightmapQueue.Count > 0) {
				finalizeHeightmap((RenderedHeightmap)heightmapQueue.Dequeue());
			}
		} else {
			if (heightmapQueue.Count > 0) {
				finalizeHeightmap((RenderedHeightmap)heightmapQueue.Dequeue());
			}
		}

	}

	void finalizeHeightmap(RenderedHeightmap rh) {
		TerrainData tData = new TerrainData();

		tData.heightmapResolution = chunkResolution;
		tData.alphamapResolution = chunkResolution;
		tData.size = new Vector3(chunkWidth, terrainHeight, chunkWidth);

		tData.splatPrototypes = splats;
    	tData.RefreshPrototypes();

		/* Create and position the terrain */
		GameObject terrain = Terrain.CreateTerrainGameObject(tData);
		terrain.transform.position = new Vector3(
				chunkWidth * rh.coord.x,
				0.0f,
				chunkWidth * rh.coord.z);

		/* Set the heightmap data from the background thread. */
		tData.SetHeights(0, 0, rh.heightmap);

		stitchTerrain(tData, rh.coord);

		chunks[rh.coord] = tData;

		// texturing
		tData.SetAlphamaps(0, 0, rh.splatmap);
	}

	private float[,] extractChunk(float[,] heightmap) {
		// Extract the central chunk heights from the full heightmap (which
		// includes the fringe).
		float[,] chunk = new float[chunkResolution, chunkResolution];
		for (int x = 0; x < chunkResolution; x++) {
			for (int z = 0; z < chunkResolution; z++) {
				chunk[x, z] = heightmap[(x + fringeSize), (z + fringeSize)];
			}
		}
		return chunk;
	}

	void ensureChunk(ChunkCoord c, bool async) {
		if (!chunks.Contains(c)) {
			chunks.Add(c, null);
			if (async) {
				this.StartCoroutineAsync(generateHeightmapAsync(c));
			} else {
				generateHeightmapSync(c);
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

	void Update () {
		drainHeightmapQueue(false);

		float px = player.transform.position.x;
		float pz = player.transform.position.z;

		int cx = (int) (px / chunkWidth);
		int cz = (int) (pz / chunkWidth);

		for (int row = 0; row <= chunkHorizon; row++) {
			int stripWidth = chunkHorizon - row;
			for (int dx = -stripWidth; dx <= stripWidth; dx++) {
				ensureChunk(new ChunkCoord(cx + dx, cz + row), true);
				ensureChunk(new ChunkCoord(cx + dx, cz - row), true);
			}
		}
		// for (int dx = -chunkHorizon; dx <= chunkHorizon; dx++) {
		// 	for (int dz = -chunkHorizon; dz <= chunkHorizon; dz++) {
		// 		ensureChunk(new ChunkCoord(cx + dx, cz + dz), true);
		// 	}
		// }
	}
}
