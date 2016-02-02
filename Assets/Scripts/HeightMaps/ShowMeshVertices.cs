using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class ShowMeshVertices : MonoBehaviour {
	void OnDrawGizmos() {

		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		Mesh mesh = mf.sharedMesh;
		Vector3[] vertices = mesh.vertices;

		Gizmos.matrix = Matrix4x4.TRS(transform.position,
			transform.rotation,
			transform.lossyScale);
		for (int i = 0; i < vertices.Length; i++) {
			Gizmos.DrawSphere(vertices[i], 0.05f);
		}
	}
}
