using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class NaiveTerrain : MonoBehaviour {
	public  int size = 4;
	public float cellSize = 1.0f;

	// Use this for initialization
	void Start () {
		Regenerate();
	}

	public void Regenerate() {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.vertices = GenVertices();
		mesh.triangles = GenTriangles();
		mesh.uv = GenUVs();
		mesh.RecalculateNormals();
	}

	Vector3[] GenVertices() {
		int w = (size+1);
		int l = (size+1);
		Vector3[] vertices = new Vector3[w*l];
		for (int gx = 0; gx < w; gx++) {
			for (int gz = 0; gz < l; gz++) {
				float height = Random.Range(-1.0f, +1.0f);
				vertices[gx*l+gz] = new Vector3(gx*cellSize, height, gz*cellSize);
			}
		}
		return vertices;
	}

	int[] GenTriangles() {
		int vertciesPerTriangle = 3;
		int trianglesPerCell = 2;
		int numberCells = size * size;
		int[] triangles = new int[vertciesPerTriangle * trianglesPerCell * numberCells];

		int tindex = 0;
		for (int cx = 0; cx < size; cx++) {
			for (int cz = 0; cz < size; cz++) {
				int n = cx*(size+1)+cz;

				triangles[tindex] = n;
				triangles[tindex+1] = n+1;
				triangles[tindex+2] = n+size+2;
				triangles[tindex+3] = n;
				triangles[tindex+4] = n+size+2;
				triangles[tindex+5] = n+size+1;
				tindex +=6;
			}
		}
		return triangles;
	}

	Vector2[] GenUVs() {
		int w = (size + 1);
		int l = (size + 1);
		Vector2[] uvs = new Vector2[w * l];

		for (int ux = 0; ux < w; ux++) {
			for (int uz = 0; uz < l; uz++) {
				uvs[ux*l+uz] = new Vector2((float)ux/size, (float)uz/size);
			}
		}
		return uvs;
	}
}

