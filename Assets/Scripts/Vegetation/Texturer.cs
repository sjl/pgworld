using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Texturer
{
	public float slopeValue;
	public float mountainPeekHeight;
	public float waterHeight;
	private int width;
	private int height;
	private Terrain terrain;
	private TerrainData terrainData;
	private List<TreeInstance> TreeInstances;

	public float[,,] Texture(float[,] heightMap, float[,,] map) {
		this.width = heightMap.GetLength(0);
		this.height = heightMap.GetLength(1);

		SlopeAndHeightTexture(heightMap, map);

		return map;
	}
								
	private void SlopeAndHeightTexture(float[,] heightMap, float[,,] map)
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

						var temp = heightMap[y, x] - heightMap[ny, nx];

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}
					
				if (locationHeight < waterHeight) {
					map[y, x, 0] = 0;
					map[y, x, 1] = 0;
					map[y, x, 2] = locationHeight;
					map[y, x, 3] = 0;
					map[y, x, 4] = 1 - locationHeight;
				} else if (locationHeight >= (mountainPeekHeight)){// + Random.Range(0f, 0.4f))) { // mountain tops texturing
					var halfDiff = (1 - mountainPeekHeight) / 2;
					var quarterDiff = halfDiff / 2;
					if (locationHeight >= (mountainPeekHeight + halfDiff + quarterDiff)) { // highest peek
						map[y, x, 0] = 0;
						map[y, x, 1] = 0;
						map[y, x, 2] = 1 - locationHeight;
						map[y, x, 3] = locationHeight;
						map[y, x, 4] = 0;
					} else if (locationHeight <= (mountainPeekHeight + halfDiff)) { // lowest peek
						map[y, x, 0] = 0;
						map[y, x, 1] = locationHeight * 0.05f;
						map[y, x, 2] = locationHeight * 0.9f;
						map[y, x, 3] = 1 - locationHeight * 0.85f;
						map[y, x, 4] = 0;
					} else { // middle peek
						map[y, x, 0] = 0;
						map[y, x, 1] = 0;
						map[y, x, 2] = 1 - locationHeight * 0.5f;
						map[y, x, 3] = locationHeight * 0.5f;
						map[y, x, 4] = 0;
					}
				} else if (maxDifference > slopeValue) { // slope texturing
					map[y, x, 0] = 0;
					map[y, x, 1] = 0.15f;
					map[y, x, 2] = 0.84f;
					map[y, x, 3] = 0.01f;
					map[y, x, 4] = 0;
				} else { // default texturing based on height
					map[y, x, 0] = 2*(1 - locationHeight)/3;
					map[y, x, 1] = (1 - locationHeight)/3;
					map[y, x, 2] = locationHeight;
					map[y, x, 3] = 0;
					map[y, x, 4] = 0;
				}
			}
		}
	}
	/*
	public void AddTrees(int width, int height, float[,] heightMap, Terrain terrain) {
		this.width = width;
		this.height = height;
		this.heightMap = heightMap;
		this.terrain = terrain;

		PopulateTreeInstances();
	}

	private void PopulateTreeInstances()
	{
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

				// read the height at this location
				float locationHeight = heightMap [y, x];

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

						var temp = heightMap [y, x] - heightMap [ny, nx];

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}

				if (locationHeight < mountainPeekHeight && maxDifference < slopeValue && locationHeight > waterHeight) { // the slope and height is within limits
					
					var rnd = Random.Range(0, (600 + ((int) locationHeight * 100))); // fewer trees as it gets higher
					if (rnd == 0) {
						// add random trees	
						var treeInstance = new TreeInstance();
						treeInstance.prototypeIndex = Random.Range(0, 3); // mix of three tree prototypes
						treeInstance.heightScale = Random.Range(0.3f, 1.2f) - (0.3f * locationHeight); // smaller trees where height is more (less o2)
						treeInstance.widthScale = treeInstance.heightScale;
						treeInstance.color = Color.white;
						treeInstance.position = new Vector3((float) x / width, 0.0f, (float) y / height);

						terrain.AddTreeInstance(treeInstance);
					}
				}
			}
		}
	}

	public void RemoveTrees(TerrainData terrainData) {
		this.terrainData = terrainData;

		RemoveTrees();
	}

	private void RemoveTrees () {
		// Get all tree instances and delete them
		TreeInstances = new List<TreeInstance> (terrainData.treeInstances);
		for (var i = 0; i < TreeInstances.Count; i++) {
			TreeInstances.RemoveAt(i);
		}
		terrainData.treeInstances = TreeInstances.ToArray();

		// Now refresh the terrain, getting rid of the darn collider
        float[,] heights = terrainData.GetHeights(0, 0, 0, 0);
        terrainData.SetHeights(0, 0, heights);
	}*/
	/*
	public void AddRocks (int width, int height, float[,] heightMap, Terrain terrain) {
		this.width = width;
		this.height = height;
		this.heightMap = heightMap;
		this.terrain = terrain;

		AddRockInstances();
	}

	private void AddRockInstances ()
	{
		var rock1 = GameObject.Find ("Generic Cliff_D");
		var rock2 = GameObject.Find ("TRP_Rock big");
		var rock3 = GameObject.Find ("TRP_Rock small");

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				var rnd = Random.Range(0, 200000);
				if (rnd != 10) continue;

				// read the height at this location
				float locationHeight = heightMap [y, x];

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

						var temp = heightMap [y, x] - heightMap [ny, nx];

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}
				var pos = new Vector3((float) x / width, 0.0f, (float) y / height);

				if (locationHeight < waterHeight) {
					//var rockHeight = terrain.SampleHeight(rock1.transform.position);
					Instantiate(rock3, new Vector3(Random.Range(200, 2000), height, Random.Range(200, 2000)), Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), transform.right));
					//rock3.transform.position = pos;
				} else if (maxDifference > slopeValue) {
					//var rockHeight = this.terrain.SampleHeight(rock1.transform.position);
					Instantiate(rock1, new Vector3(Random.Range(200, 2000), height, Random.Range(200, 2000)), Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), transform.right));
					//rock1.transform.position = pos;
				} else {
					//var rockHeight = this.terrain.SampleHeight(rock1.transform.position);
					Instantiate(rock2, new Vector3(Random.Range(200, 2000), height, Random.Range(200, 2000)), Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), transform.right));
					//rock2.transform.position = pos;
				}
			}
		}*/
		/*
		var rocksToSpawn = 2;
		for (int i = 0; i < rocksToSpawn; i++) {
             float height = this.terrain.SampleHeight(rock1.transform.position);
             Vector3 randSpawn = new Vector3(Random.Range(200, 2000), height, Random.Range(200, 2000));
             Instantiate(rock1, randSpawn, Quaternion.AngleAxis(Random.Range(-180.0f, 180.0f), transform.right));
             rock1.transform.position = new Vector3(rock1.transform.position.x, height, rock1.transform.position.z);
        }
	}*/
}