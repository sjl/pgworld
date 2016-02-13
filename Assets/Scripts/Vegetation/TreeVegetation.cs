using UnityEngine;
using System.Collections;

public class TreeVegetation : Vegetation
{
	private int width;
	private int height;
	private float[,] heightMap;
	private Terrain terrain;

	public override void AddTrees(int width, int height, float[,] heightMap, Terrain terrain) {
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

				if (locationHeight < 0.9 && maxDifference < 0.0029) { // the slope and height is within limits
					var rnd = Random.Range (0, 600);

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

	public override float[,,] Texture (int width, int height, float[,] heightMap, float[,,] map) {
		return null;
	}
}