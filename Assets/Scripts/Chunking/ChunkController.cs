using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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
	public readonly List<TreeInstance> treeInstances;
	public RenderedHeightmap(ChunkCoord c, float[,] h, float[,,] sm, List<TreeInstance> ti) {
		coord 			= c;
		heightmap 		= h;
		splatmap 		= sm;
		treeInstances 	= ti;
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
	public float shoreHeight = 0.001f;

	public int reverseErosionIterations = 10;
	public float reverseErosionTalus = 0.001f;
	public float reverseErosionStrength = 1.0f;
	public float reverseErosionReverseTalusCutoff = 0.0001f;

	public float hydraulicErosionRainfallAmount = 0.01f; // kr
	public float hydraulicErosionEvaporationRatio = 0.5f; // ke
	public float hydraulicErosionSedimentCapacity = 0.01f; // kc
	public float hydraulicErosionSoilSolubility = 0.01f; // ks
	public float hydraulicErosionRainAltitude = 0.4f;
	public float hydraulicErosionRainFalloff = 1.0f;
	public int hydraulicErosionIterations = 1;

	public int thermalErosionIterations = 10;
	public float thermalErosionTalus = 0.001f;
	public float thermalErosionStrength = 1.0f;

	public GameObject[] treePrefab;
	private TreePrototype[] treeProto;
	public int treeStrength = 10;
	public float treeHeightFactor = 2.5f;

	private int grassType = 0;
	public Texture2D grass;
	private DetailPrototype[] grassProto;
    public int[] texturesToAffect;


	private int chunkResolution;
	private float chunkWidth;

	private PerlinNoise noise;
	private ThermalTerrainErosion reverseThermalEroder;
	private ThermalTerrainErosion thermalEroder;
	private HydraulicTerrainErosion hydraulicEroder;
	private Texturer texturer;
	private GrassPlacement grassPlacer;

	// CHUNKING AND THREADING
	//
	// We store which chunks we've added in a hashmap so we can check them
	// quickly in each frame.
	//
	// We don't actually add the chunks during the frame Update().  Instead, we
	// fire off a background thread to come up with the initial noised, eroded,
	// textured heightmap.
	//
	// Once that thread finishes it shoves the result into a queue, which the
	// main thread consumes from once per frame.
	private Hashtable chunks;
	private Queue heightmapQueue;

	// THREAD LIMITING
	//
	// Doing work in the background with threads is all well and good, but we
	// need to be careful.  If we just spin up 50 background threads they'll
	// thrash and work against each other and end up slower in the end. Worse
	// still: they'll compete with the main thread!  So we need to limit the
	// number of outstanding worker threads at any given time.
	//
	// This limit should be set to (roughly) your number of CPU cores, minus
	// one or two for the main thread and anything else your computer might want
	// to do besides playing the game.
	private int outstanding = 0;
	public int threads = 6;

	void Start ()
	{
		noise = new PerlinNoise ();
		noise.baseOctave = noiseBaseOctave;
		noise.weights = noiseWeights;
		noise.Init ();

		chunkResolution = (int)Math.Pow (2, chunkExponent) + 1;
		chunkWidth = chunkResolution * chunkScale;

		chunks = new Hashtable ();
		heightmapQueue = Queue.Synchronized (new Queue ());

		reverseThermalEroder = new ThermalTerrainErosion ();
		reverseThermalEroder.talus = this.reverseErosionTalus;
		reverseThermalEroder.strength = this.reverseErosionStrength;
		reverseThermalEroder.reverse = true;
		reverseThermalEroder.reverseTalusCutoff = this.reverseErosionReverseTalusCutoff;
		reverseThermalEroder.iterations = this.reverseErosionIterations;

		hydraulicEroder = new HydraulicTerrainErosion ();
		hydraulicEroder.rainfallAmount = hydraulicErosionRainfallAmount; // kr
		hydraulicEroder.evaporationRatio = hydraulicErosionEvaporationRatio; // ke
		hydraulicEroder.sedimentCapacity = hydraulicErosionSedimentCapacity; // kc
		hydraulicEroder.soilSolubility = hydraulicErosionSoilSolubility; // ks
		hydraulicEroder.rainAltitude = hydraulicErosionRainAltitude;
		hydraulicEroder.rainFalloff = hydraulicErosionRainFalloff;
		hydraulicEroder.iterations = hydraulicErosionIterations;

		thermalEroder = new ThermalTerrainErosion ();
		thermalEroder.talus = this.thermalErosionTalus;
		thermalEroder.strength = this.thermalErosionStrength;
		thermalEroder.reverse = false;
		thermalEroder.iterations = this.thermalErosionIterations;

		texturer = new Texturer ();
		texturer.slopeValue = slopeValue;
		texturer.mountainPeekHeight = mountainPeekHeight;
		texturer.waterHeight = waterHeight;
		texturer.shoreHeight = shoreHeight;
		splats = new SplatPrototype[diffuses.Length];

		texturer.treeHeightFactor = treeHeightFactor;
		texturer.treeStrength = treeStrength;
		treeProto = new TreePrototype[treePrefab.Length];
		for (var i = 0; i < treePrefab.Length; i++) {
			treeProto[i] = new TreePrototype();
			treeProto[i].bendFactor = 0;
			treeProto[i].prefab = treePrefab[i];
		}

		grassPlacer = new GrassPlacement();
		grassPlacer.grassType = grassType;
		grassPlacer.texturesToAffect = texturesToAffect;

		for (var i = 0; i < diffuses.Length; i++) {
			splats[i] = new SplatPrototype();
			splats[i].texture = diffuses[i];
			splats[i].normalMap = normals[i];
			splats[i].tileSize = new Vector2(5, 5);
		}
		grassProto = new DetailPrototype[1];
		grassProto[0] = new DetailPrototype();
		grassProto[0].prototypeTexture = grass;
		grassProto[0].bendFactor = 0.1f;
		grassProto[0].dryColor = new Color(0.804f, 0.737f, 0.102f, 1.000f);
		grassProto[0].healthyColor = new Color(0.263f, 0.976f, 0.165f, 1.000f);
		grassProto[0].maxHeight = 0.5f;
		grassProto[0].minHeight = 0.2f;
		grassProto[0].maxWidth = 1f;
		grassProto[0].minWidth = 0.5f;
		grassProto[0].noiseSpread = 0.1f;
		grassProto[0].prototype = null;
		grassProto[0].renderMode = DetailRenderMode.GrassBillboard;
		grassProto[0].usePrototypeMesh = false;

		ensureChunk(new ChunkCoord(0, 0), false);
		ensureChunk(new ChunkCoord(-1, -1), true);
		ensureChunk(new ChunkCoord(0, -1), true);
		ensureChunk(new ChunkCoord(-1, 0), true);
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
		hydraulicEroder.Erode(heightmap);
		thermalEroder.Erode(heightmap);
	}

	float[,,] textureHeightmap(float[,] heightmap, int[,] randomArray) {
		// determain the mix of textures 1, 2, 3 and 4 to use
		var splatmap = new float[heightmap.GetLength(0), heightmap.GetLength(1), diffuses.Length];

		splatmap = texturer.Texture(heightmap, splatmap, randomArray);

		return splatmap;
	}

	List<TreeInstance> treePlacement (float[,] heightmap, int[,] randomArray) {
		var treeInstances = new List<TreeInstance>();
		treeInstances = texturer.AddTrees(heightmap, treeInstances, randomArray);

		return treeInstances;
	}

	int[,] grassPlacement(int detailWidth, float[,,] splatmap) {
		int[,] newDetailLayer = new int[detailWidth, detailWidth];
		grassPlacer.RunGrassGenerator(newDetailLayer, splatmap);

		return newDetailLayer;
	}

	void generateHeightmapSync(ChunkCoord c, int[,] randomArray) {
		int heightmapResolution = chunkResolution + (2 * fringeSize);
		float[,] heightmap = new float[heightmapResolution, heightmapResolution];

		noiseHeightmap(c, heightmap);
		erodeHeightmap(c, heightmap);

		heightmap = extractChunk(heightmap);

		var splatmap = textureHeightmap(heightmap, randomArray);

		var treeInstances = treePlacement(heightmap, randomArray);

		// Done, push it into the queue so the main thread can process it into
		// a terrain object.
		heightmapQueue.Enqueue(new RenderedHeightmap(c, heightmap, splatmap, treeInstances));
	}

	IEnumerator generateHeightmapAsync(ChunkCoord c, int[,] randomArray) {
		// This is called on a background thread.  It needs to build the
		// heightmap array (and possibly some other stuff) and push it onto the
		// queue when ready.

		generateHeightmapSync(c, randomArray);

		yield return Ninja.JumpToUnity;

		//print("Outstanding: " + outstanding);
		outstanding -= 1;

		yield return Ninja.JumpBack;
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

	void finalizeHeightmap (RenderedHeightmap rh)
	{
		TerrainData tData = new TerrainData();

		tData.heightmapResolution = chunkResolution;
		tData.alphamapResolution  = chunkResolution;
		tData.SetDetailResolution(chunkResolution*2, 1);
		tData.size = new Vector3(chunkWidth, terrainHeight, chunkWidth);

		tData.splatPrototypes  = splats;
		tData.detailPrototypes = grassProto;
		tData.treePrototypes   = treeProto;
		tData.RefreshPrototypes();

		// tree placement
		tData.treeInstances = rh.treeInstances.ToArray();

		/* Create and position the terrain */
		GameObject terrain = Terrain.CreateTerrainGameObject(tData);
		terrain.transform.position = new Vector3 (
			chunkWidth * rh.coord.x,
			0.0f,
			chunkWidth * rh.coord.z);

		/* Set the heightmap data from the background thread. */
		tData.SetHeights(0, 0, rh.heightmap);

		stitchTerrain(tData, rh.coord);

		chunks[rh.coord] = tData;

		// texturing
		tData.SetAlphamaps(0, 0, rh.splatmap);

		// grass placement
		/*if (texturesToAffect.Length != 0) {
			var detailedLayer = grassPlacement (tData.detailResolution, tData.GetAlphamaps (0, 0,
				                   tData.alphamapWidth, 
				                   tData.alphamapHeight));
			tData.SetDetailLayer (0, 0, grassType, detailedLayer);
		}*/
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
		if (outstanding < threads) {
			if (!chunks.Contains(c)) {
				chunks.Add(c, null);

				// RandomRangeInt can only be called from the main thread.
				// So we need to pass an array of random numbers to the child thread
				int heightmapResolution = chunkResolution + (2 * fringeSize);
				var randomArray = new int[heightmapResolution, heightmapResolution];
				for (var i = 0; i < heightmapResolution; i++) {
					for (var j = 0; j < heightmapResolution; j++) {
						randomArray[i,j] = UnityEngine.Random.Range(0, 600);
					}
				}

				if (async) {
					outstanding += 1;
					this.StartCoroutineAsync(generateHeightmapAsync(c, randomArray));
				} else {
					generateHeightmapSync(c, randomArray);
				}
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
