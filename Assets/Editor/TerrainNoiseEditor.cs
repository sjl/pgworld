using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainNoise))]
public class TerrainNoiseEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		TerrainNoise noiser = (TerrainNoise)target;
		if (GUILayout.Button("Run")) {
			noiser.Run();
		}
	}
}
