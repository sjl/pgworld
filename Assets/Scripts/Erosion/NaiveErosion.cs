using UnityEngine;
using System.Collections;

public class NaiveErosion : Erosion {
	public float divisor = 2.0f;

	public override Vector3[] Erode(Vector3[] current, int sizeX, int sizeZ) {		
		float x, y, z;
		int width = (sizeX+1);
		int length = (sizeZ+1);

		Vector3[] vertices = new Vector3[width * length];

		for (int xcoord = 0; xcoord < width; xcoord++) {
			for (int zcoord = 0; zcoord < length; zcoord++) {
				int idx = xcoord * length + zcoord;

				x = current[idx][0];
				y = current[idx][1];
				z = current[idx][2];
				vertices[idx] = new Vector3(x, y/divisor, z);
			}
		}
		return vertices;
	}
}
