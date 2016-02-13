using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Noise))]
public class CustomTerrain : MonoBehaviour {
	public int sizeX = 4;
	public int sizeZ = 4;
	public float noiseSize = 1.0f; 
	public float cellSize = 1.0f;
	private string mPath = "Assets/Terrains/Generated/";

	Noise noise;

	void Start() {
		noise = GetComponent<Noise>();
	}

	public void Regenerate() {
		noise.Init();
		Mesh mesh = EnsureMesh();
		mesh.vertices = GenVertices();
		FixMesh();
	}

	public void recalculateMeshCollider(Mesh mesh) {
		if (GetComponent<MeshCollider>()) {
			DestroyImmediate(GetComponent<MeshCollider>());
		}
		transform.gameObject.AddComponent<MeshCollider>();
		transform.GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	public void SaveMesh() {
		Mesh mesh = Instantiate<Mesh>(GetComponent<MeshFilter>().sharedMesh);
		if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(mPath))) {
			AssetDatabase.CreateFolder("Assets/Terrains", "Generated");
		}
		AssetDatabase.CreateAsset(mesh, mPath + gameObject.name + ".asset");
	}

	public void LoadMesh() {
		Mesh mesh = Instantiate(AssetDatabase.LoadMainAssetAtPath(mPath + gameObject.name + ".asset") as Mesh);
		if (mesh) {
			GetComponent<MeshFilter>().mesh = mesh;
		}
		recalculateMeshCollider(mesh);
	}

	void FixMesh() {
		// Bookkeeping stuff that needs to be rerun after you modify
		// the vertices of a mesh.
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		mesh.triangles = GenTriangles();
		mesh.uv = GenUVs();
		mesh.RecalculateNormals();
		recalculateMeshCollider(mesh);
	}

	Mesh EnsureMesh() {
		// Make sure we have a mesh, creating it if necessary, and return it.
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		if(!mesh) {
			mesh = new Mesh();
			GetComponent<MeshFilter>().sharedMesh = mesh;
		}
		return mesh;
	}

	Vector3[] GenVertices() {
		float x, z;
		int w = (sizeX+1);
		int l = (sizeZ+1);
		Vector3[] vertices = new Vector3[w*l];
		for (int gx = 0; gx < w; gx++) {
			for (int gz = 0; gz < l; gz++) {
				x = gx*cellSize;
				z = gz*cellSize;
				float height = (noiseSize * noise.Get(x,z));
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

