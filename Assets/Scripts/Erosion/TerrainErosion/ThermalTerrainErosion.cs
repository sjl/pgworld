using UnityEngine;
using System.Collections;

public class ThermalTerrainErosion : TerrainErosion {
	public float talus = 0.01f;
	public float strength = 0.2f;
	public bool reverse = false;
	public float reverseTalusCutoff = 0.003f;
	public int iterations = 1;

	private float[,] heightmap;
	private int width, height;

	void ErodeHeightmap() {

		// perform a single thermal erosion of the heightmap.
		for (int zcoord = 0; zcoord < height; zcoord++) {
			for (int xcoord = 0; xcoord < width; xcoord++) {
				float difference;
				float maxDifference = 0.0f;
				int targetx = xcoord;
				int targetz = zcoord;

				// Use a full Moore neighborhood.  A Von Neumann is faster but can result
				// in weird artifacts sometimes.
				for (int dx = -1; dx <= 1; dx += 1) {
					for (int dz = -1; dz <= 1; dz += 1) {
						int nx = xcoord + dx;
						int nz = zcoord + dz;

						if (nx < 0 || nx >= width || nz < 0 || nz >= height || (nx == 0 && nz == 0)) {
							// Skip neighbor cells that fall off the map.
							continue;
						}

						difference = heightmap[zcoord, xcoord] - heightmap[nz, nx];
						if (maxDifference < difference) {
							maxDifference = difference;
							targetx = nx;
							targetz = nz;
						}
					}
				}

				// Only move material to/from the lowest neighbor.  This is much faster
				// than moving a bit to all the neighbors at once, and it converges to
				// roughly the same thing anyway.
				//
				// We also allow for "reverse thermal erosion" by inverting the check.
				bool shouldMove = reverse
					? (maxDifference <= talus && maxDifference >= reverseTalusCutoff)
					: maxDifference > talus;
				
				if (shouldMove) {
					float toMove = maxDifference / 2.0f * strength;
					heightmap[zcoord, xcoord] -= toMove;
					heightmap[targetz, targetx] += toMove;
				}

			}
		}
	}

	public override float[,] Erode(float[,] heightmap, int width, int height) {
		this.heightmap = heightmap;
		this.width = width;
		this.height = height;

		for (int i = 0; i < iterations; i++) {
			ErodeHeightmap();
		}

		return this.heightmap;
	}
}
