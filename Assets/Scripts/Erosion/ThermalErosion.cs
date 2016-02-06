using UnityEngine;
using System.Collections;

public class ThermalErosion : Erosion {
	public float talus = 0.5f;
	public bool reverse = false;
	public int iterations = 1;

	private float[,] heightmap;
	private int width, length;

	void FillInitialHeightmap(Vector3[] vertices) {
		heightmap = new float[width, length];

		for (int xcoord = 0; xcoord < width; xcoord++) {
			for (int zcoord = 0; zcoord < length; zcoord++) {
				int idx = xcoord * length + zcoord;
				heightmap[xcoord, zcoord] = vertices[idx][1];
			}
		}
	}

	void ErodeHeightmap() {
		// perform a single thermal erosion of the heightmap.
		for (int xcoord = 0; xcoord < width; xcoord++) {
			for (int zcoord = 0; zcoord < length; zcoord++) {
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

						if (nx < 0 || nx >= width || nz < 0 || nz >= length || (nx == 0 && nz == 0)) {
							// Skip neighbor cells that fall off the map.
							continue;
						}

						difference = heightmap[xcoord, zcoord] - heightmap[nx, nz];
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
				if (reverse ? maxDifference <= talus : maxDifference > talus) {
					float toMove = maxDifference / 2.0f * 0.5f;
					heightmap[xcoord, zcoord] -= toMove;
					heightmap[targetx, targetz] += toMove;
				}

			}
		}
	}

	public override Vector3[] Erode(Vector3[] current, int sizeX, int sizeZ) {
		float x, y, z;
		width = (sizeX+1);
		length = (sizeZ+1);

		FillInitialHeightmap(current);
		for (int i = 0; i < iterations; i++) {
			ErodeHeightmap();
		}

		Vector3[] vertices = new Vector3[width * length];
		for (int xcoord = 0; xcoord < width; xcoord++) {
			for (int zcoord = 0; zcoord < length; zcoord++) {
				int idx = xcoord * length + zcoord;

				x = current[idx][0];
				y = heightmap[xcoord, zcoord];
				z = current[idx][2];
				vertices[idx] = new Vector3(x, y, z);
			}
		}
		return vertices;
	}
}
