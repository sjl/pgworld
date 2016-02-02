using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NaiveTerrain))]
public class NaiveTerrainEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		NaiveTerrain nt = (NaiveTerrain)target;
		if (GUILayout.Button("Regenerate")) {
			nt.Regenerate();
		}
	}
}
