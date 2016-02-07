using UnityEngine;
using System.Collections;

public class NaiveTerrainErosion : TerrainErosion {
	public float divisor = 2.0f;

	public override float[,] Erode(float[,] heightmap, int width, int height) {
		for (int zcoord = 0; zcoord < height; zcoord++) {
			for (int xcoord = 0; xcoord < width; xcoord++) {
				float y = heightmap[zcoord, xcoord];
				heightmap[zcoord, xcoord] = y / divisor;
			}
		}
		return heightmap;
	}
}
