using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Vegetation))]
public class VegetationEditor : Editor {}

[CustomEditor(typeof(TextureVegetation))]
public class TextureVegetationEditor : VegetationEditor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		Vegetation vegetation = (Vegetation)target;

		if (GUILayout.Button("Run texturing")) {
			vegetation.RunTexturing();
		}
	}
}

[CustomEditor(typeof(TreeVegetation))]
public class TreeVegetationEditor : VegetationEditor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		Vegetation vegetation = (Vegetation)target;

		if (GUILayout.Button("Run tree generator")) {
				vegetation.RunTreeGenerator();
		}
	}
}