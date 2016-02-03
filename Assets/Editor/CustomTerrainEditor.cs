using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrainEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		CustomTerrain nt = (CustomTerrain)target;
		if (GUILayout.Button("Regenerate")) {
			nt.Regenerate();
		}

		if (GUILayout.Button("Save")) {
			nt.SaveMesh();
		}

		if (GUILayout.Button("Load")) {
			nt.LoadMesh();
		}
	}

}
