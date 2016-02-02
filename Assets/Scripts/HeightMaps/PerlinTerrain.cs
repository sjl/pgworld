using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class PerlinTerrain : MonoBehaviour {
	public  int sizeX = 4;
	public  int sizeZ = 4;
	public float cellSize = 1.0f;

	// Use this for initialization
	void Start () {
	}

	public void Regenerate() {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.vertices = GenVertices();
		mesh.triangles = GenTriangles();
		mesh.uv = GenUVs();
		mesh.RecalculateNormals();
		if (GetComponent<MeshCollider>()) {
			DestroyImmediate(GetComponent<MeshCollider>());
		}
		transform.gameObject.AddComponent<MeshCollider>();
		transform.GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	Vector3[] GenVertices() {
		PerlinCalculator noise = new PerlinCalculator();

		float x, z;
		int w = (sizeX+1);
		int l = (sizeZ+1);
		Vector3[] vertices = new Vector3[w*l];
		for (int gx = 0; gx < w; gx++) {
			for (int gz = 0; gz < l; gz++) {
				x = gx*cellSize;
				z = gz*cellSize;
				float height = 3.0f * noise.Get(x,z);
				vertices[gx*l+gz] = new Vector3(x, height, z);
			}
		}
		return vertices;
	}

	int[] GenTriangles() {
		int vertciesPerTriangle = 3;
		int trianglesPerCell = 2;
		int numberCells = sizeX * sizeZ;
		int[] triangles = new int[vertciesPerTriangle * trianglesPerCell * numberCells];

		int tIndeX = 0;
		for (int cX = 0; cX < sizeX; cX++) {
			for (int cZ = 0; cZ < sizeZ; cZ++) {
				int n = cX*(sizeZ+1)+cZ;

				triangles[tIndeX] = n;
				triangles[tIndeX+1] = n+1;
				triangles[tIndeX+2] = n+sizeZ+2;
				triangles[tIndeX+3] = n;
				triangles[tIndeX+4] = n+sizeZ+2;
				triangles[tIndeX+5] = n+sizeZ+1;
				tIndeX +=6;
			}
		}
		return triangles;
	}

	Vector2[] GenUVs() {
		int w = (sizeX + 1);
		int l = (sizeZ + 1);
		Vector2[] uvs = new Vector2[w * l];

		for (int uX = 0; uX < w; uX++) {
			for (int uZ = 0; uZ < l; uZ++) {
				uvs[uX*l+uZ] = new Vector2((float)uX/sizeX, (float)uZ/sizeZ);
			}
		}
		return uvs;
	}
}

