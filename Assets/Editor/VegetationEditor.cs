using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Vegetation))]
public class VegetationEditor : Editor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector ();

		Vegetation vegetation = (Vegetation)target;
		if (GUILayout.Button ("Run")) {
			vegetation.Run();
		}
	}
}
