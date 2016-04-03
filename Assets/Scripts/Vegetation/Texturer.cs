using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Texturer
{
	const int GRASS_LOW_A = 0;
	const int GRASS_LOW_B = 1;
	const int GRASS_HIGH = 2;
	const int SLOPE_A = 3;
	const int SLOPE_B = 4;
	const int SNOW = 5;
	const int UNDERWATER = 6;

	public float slopeValue = 0.002f;
	public float mountainPeekStart = 0.51f;
	public float mountainPeekHeight = 0.52f;
	public float waterHeight = 0.44f;
	public float shoreHeight;
	public float treeHeightFactor;
	public int treeStrength;

	private int width;
	private int height;
	private Terrain terrain;
	private TerrainData terrainData;
	private List<TreeInstance> TreeInstances;

	private PerlinNoise textureBlendNoise;
	private float textureBlendNoiseBaseOctave = 0.5f;
	private float[] textureBlendNoiseWeights = {
		100.0f, 1.0f
	};

	public void Init() {
		textureBlendNoise = new PerlinNoise();
		textureBlendNoise.baseOctave = textureBlendNoiseBaseOctave;
		textureBlendNoise.weights = textureBlendNoiseWeights;
		textureBlendNoise.Init();
	}

	public float[,,] Texture(
			float[,] heightMap, float[,,] map,
			int[,] randomArray, int ox, int oy) {
		this.width = heightMap.GetLength(0);
		this.height = heightMap.GetLength(1);

		SlopeAndHeightTexture(heightMap, map, randomArray, ox, oy);

		return map;
	}

	private void SlopeAndHeightTexture(
			float[,] heightMap, float[,,] map, int[,] randomArray,
			int ox, int oy)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

				// read the height at this location
				float locationHeight = heightMap[y, x];

				// used for slope texturing
				var maxDifference = 0.0f;

				// Use a full Moore neighborhood.  A Von Neumann is faster but can result
				// in weird artifacts sometimes.
				for (int dx = -1; dx <= 1; dx += 1) {
					for (int dy = -1; dy <= 1; dy += 1) {
						int nx = x + dx;
						int ny = y + dy;

						if (nx < 0 || ny < 0 || nx >= width || ny >= height || (nx == 0 && ny == 0)) {
							// Skip neighbor cells that fall off the map.
							continue;
						}

						var temp = Mathf.Abs(heightMap[y, x] - heightMap[ny, nx]);

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}

				var slopeNoiseSpeed = 0.25f;
				var slopeNoise = PGMath.clamp(PGMath.expand(
							textureBlendNoise.Get(
								(ox + x) * slopeNoiseSpeed,
								(oy + y) * slopeNoiseSpeed
							), 0.10f),
						0.0f, 1.0f);

				if (locationHeight < (waterHeight * 0.99f)) {
					map[y, x, GRASS_LOW_A] = 0;
					map[y, x, GRASS_LOW_B] = 0;
					map[y, x, GRASS_HIGH] = 0;
					map[y, x, SLOPE_A] = 0.1f;
					map[y, x, SLOPE_B] = 0.1f;
					map[y, x, SNOW] = 0;
					map[y, x, UNDERWATER] = 0.8f;
				} else if (locationHeight >= mountainPeekHeight){ // mountain tops texturing
					var noiseSpeed = 0.2f;
					var noise = PGMath.clamp(PGMath.expand(
								textureBlendNoise.Get(
									(ox + x) * noiseSpeed, (oy + y) * noiseSpeed
								), 0.1f),
							0.0f, 1.0f);

					float lerp = Mathf.Lerp(0.3f, 1.0f, noise);
					lerp += 10 * (locationHeight - mountainPeekHeight);
					lerp = PGMath.clamp(lerp, 0.0f, 1.0f);

					var slope = 1 - lerp;
					float slopeA = Mathf.Lerp(0.0f, slope, slopeNoise);

					map[y, x, GRASS_LOW_A] = 0;
					map[y, x, GRASS_LOW_B] = 0;
					map[y, x, GRASS_HIGH] = 0;
					map[y, x, SLOPE_A] = slopeA;
					map[y, x, SLOPE_B] = (slope - slopeA);
					map[y, x, SNOW] = lerp;
					map[y, x, UNDERWATER] = 0;
				} else if (locationHeight >= mountainPeekStart){ // mountain tops texturing
					var noiseSpeed = 0.2f;
					var noise = PGMath.clamp(PGMath.expand(
								textureBlendNoise.Get(
									(ox + x) * noiseSpeed, (oy + y) * noiseSpeed
								), 0.1f),
							0.0f, 1.0f);

					float lerp = (locationHeight - mountainPeekStart) / (mountainPeekHeight - mountainPeekStart);
					lerp *= noise;

					var slope = 1 - lerp;
					float slopeA = Mathf.Lerp(0.0f, slope, slopeNoise);

					map[y, x, GRASS_LOW_A] = 0;
					map[y, x, GRASS_LOW_B] = 0;
					map[y, x, GRASS_HIGH] = 0;
					map[y, x, SLOPE_A] = slopeA;
					map[y, x, SLOPE_B] = (slope - slopeA);
					map[y, x, SNOW] = lerp;
					map[y, x, UNDERWATER] = 0;
				} else if (maxDifference > slopeValue) { // slope texturing
					var slope = 0.84f;
					float slopeA = Mathf.Lerp(0.0f, slope, slopeNoise);

					map[y, x, GRASS_LOW_A] = 0;
					map[y, x, GRASS_LOW_B] = 0;
					map[y, x, GRASS_HIGH] = 0.15f;
					map[y, x, SLOPE_A] = slopeA;
					map[y, x, SLOPE_B] = (slope - slopeA);
					map[y, x, SNOW] = 0.01f;
					map[y, x, UNDERWATER] = 0;
				} else if (locationHeight - shoreHeight < waterHeight) {
					var slope = locationHeight;
					float slopeA = Mathf.Lerp(0.0f, slope, slopeNoise);

					map[y, x, GRASS_LOW_A] = 0;
					map[y, x, GRASS_LOW_B] = 0;
					map[y, x, GRASS_HIGH] = 0;
					map[y, x, SLOPE_A] = slopeA;
					map[y, x, SLOPE_B] = (slope - slopeA);
					map[y, x, SNOW] = 0;
					map[y, x, UNDERWATER] = 1 - locationHeight;
				} else { // default texturing based on height
					var noiseSpeed = 0.05f;
					var noise = PGMath.clamp(PGMath.expand(
								textureBlendNoise.Get(
									(ox + x) * noiseSpeed, (oy + y) * noiseSpeed
								), 0.15f),
							0.0f, 1.0f);

					var lerp = (locationHeight - waterHeight)
						/ (mountainPeekStart - waterHeight);

					lerp = lerp * lerp;

					float grass_low = (1 - lerp);

					float a = Mathf.Lerp(0.0f, grass_low, noise);

					var slope = lerp;
					float slopeA = Mathf.Lerp(0.0f, slope, slopeNoise);

					map[y, x, GRASS_LOW_A] = a;
					map[y, x, GRASS_LOW_B] = grass_low - a;
					map[y, x, GRASS_HIGH] = 0;
					map[y, x, SLOPE_A] = slopeA;
					map[y, x, SLOPE_B] = (slope - slopeA);
					map[y, x, SNOW] = 0;
					map[y, x, UNDERWATER] = 0;
				}
			}
		}
	}

	public List<TreeInstance> AddTrees(float[,] heightMap, List<TreeInstance> treeInstances, int[,] randomArray) {
		this.width = heightMap.GetLength(0);
		this.height = heightMap.GetLength(1);

		PopulateTreeInstances(heightMap, treeInstances, randomArray);

		return treeInstances;
	}

	private void PopulateTreeInstances (float[,] heightMap, List<TreeInstance> treeInstances, int[,] randomArray)
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

				// read the height at this location
				float locationHeight = heightMap[y, x];

				// used for slope texturing
				var maxDifference = 0.0f;

				// Use a full Moore neighborhood.  A Von Neumann is faster but can result
				// in weird artifacts sometimes.
				for (int dx = -1; dx <= 1; dx += 1) {
					for (int dy = -1; dy <= 1; dy += 1) {
						int nx = x + dx;
						int ny = y + dy;

						if (nx < 0 || ny < 0 || nx >= width || ny >= height || (nx == 0 && ny == 0)) {
							// Skip neighbor cells that fall off the map.
							continue;
						}

						var temp = Mathf.Abs(heightMap[y, x] - heightMap[ny, nx]);

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}

				if (locationHeight < mountainPeekStart && maxDifference < slopeValue && locationHeight > waterHeight) {
					int rnd = randomArray[y, x];

					// random location of trees	
					var temp = Mathf.Floor(locationHeight * (float)treeStrength) - 0.4f * (float)treeStrength;
					float rndBasedOnLocation = (float)rnd;
					if (temp != 0) { 
						rndBasedOnLocation += (float)treeStrength / 2.0f;
					} 

					if (rndBasedOnLocation < (float)treeStrength) {
						// smaller trees where the altitude increases (less o2)
						float rndHeight = treeHeightFactor - (rnd % treeHeightFactor);
						if (treeStrength < 3) {
							rnd += (int)temp;
						}
						int rndPrototype = (y + x) % 3; // mix of three tree prototypes

						// add random trees	
						var tI = new TreeInstance();
						tI.prototypeIndex = rndPrototype;
						tI.heightScale = rndHeight;
						tI.widthScale = tI.heightScale;
						tI.color = Color.white;
						tI.position = new Vector3((float) x / width, -0.1f, (float) y / height);

						treeInstances.Add(tI);
					}
				}
			}
		}
	}

}
