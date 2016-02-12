using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class Vegetation: MonoBehaviour {
	TerrainData terrainData;
	Terrain terrain;

	// Use this for initialization
	void Start () {
		
	}

	public void Run ()
	{
		// Get a reference to the terrain
		terrain = GetComponent<Terrain> ();

		// Get a reference to the terrain data
		terrainData = terrain.terrainData;

		// determain the mix of textures 1, 2, 3 and 4 to use
		var map = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 4];
		/*
		// For each point on the alphamap...
		for (var y = 0; y < terrainData.alphamapHeight; y++) {
			for (var x = 0; x < terrainData.alphamapWidth; x++) {
				// Get the normalized terrain coordinate that
				// corresponds to the the point.
				float height = terrainData.GetHeight (x, y);
				var normX = x * 1.0 / (terrainData.alphamapWidth - 1);
				var normY = y * 1.0 / (terrainData.alphamapHeight - 1);
				
				// Get the steepness value at the normalized coordinate.
				var angle = terrainData.GetSteepness((float) normX, (float) normY);

				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				var frac = angle / 40.0;

				// now assign the values to the correct location in the array
				/*map[x, y, 0] = (float) (1.0 - frac / 4.0);
				map[x, y, 1] = (float) (1.0 - frac / 2.0);
				map[x, y, 2] = (float) frac;
			}
		}
		
		terrainData.SetAlphamaps(0, 0, map);*/

		float[, ,] splatmapData = new float[terrainData.alphamapWidth, 
											terrainData.alphamapHeight, 
											terrainData.alphamapLayers];

		var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

		for (int y = 0; y < terrainData.alphamapHeight; y++) {
			for (int x = 0; x < terrainData.alphamapWidth; x++) {
				// read the height at this location
				float height = heightMap[y, x];

				if (height > 0.9) {
					map[y,x,0] = 0;
					map[y,x,1] = 0;
					map[y,x,2] = 1 - height + 0.7f;
					map[y,x,3] = height - 0.7f;
				} else {
					map[y,x,0] = 2*(1 - height)/3;
					map[y,x,1] = (1 - height)/3;
					map[y,x,2] = height;
					map[y,x,3] = 0;
				}
			}
		}

		// finally assign the new splatmap to the terrainData
		// terrainData.SetAlphamaps(0, 0, splatmapData);
		terrainData.SetAlphamaps(0, 0, map);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
