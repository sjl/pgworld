using System;
using System.Collections;

public class TextureVegetation : Vegetation
{
	private int width;
	private int height;
	private float[,] heightMap;
	private float[,,] map;

	public override float[,,] Textured(int width, int height, float[,] heightMap, float[,,] map) {
		this.width = width;
		this.height = height;
		this.heightMap = heightMap;
		this.map = map;

		SlopeAndHeightTexture();

		return this.map;
	}
								
	private void SlopeAndHeightTexture ()
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

						var temp = heightMap[y, x] - heightMap[ny, nx];

						if (temp > maxDifference) {
							maxDifference = temp;
						}
					}
				}

				if (locationHeight > 0.9) { // mountain tops texturing
					if (height > 0.96f) {
						map [y, x, 0] = 0;
						map [y, x, 1] = 0;
						map [y, x, 2] = 1 - locationHeight;
						map [y, x, 3] = locationHeight;
					} else if (locationHeight < 0.94f) {
						map [y, x, 0] = 0;
						map [y, x, 1] = 0;
						map [y, x, 2] = locationHeight;
						map [y, x, 3] = 1 - locationHeight;
					} else {
						map [y, x, 0] = 0;
						map [y, x, 1] = 0;
						map [y, x, 2] = 1 - locationHeight + 0.7f;
						map [y, x, 3] = locationHeight - 0.7f;
					}
				} else if (maxDifference > 0.0029) { // slope texturing
					map [y, x, 0] = 0;
					map [y, x, 1] = 0.15f;
					map [y, x, 2] = 0.84f;
					map [y, x, 3] = 0.01f;
				} else { // default texturing based on height
					map[y,x,0] = 2*(1 - locationHeight)/3;
					map[y,x,1] = (1 - locationHeight)/3;
					map[y,x,2] = locationHeight;
					map[y,x,3] = 0;
				}
			}
		}
	}
}

