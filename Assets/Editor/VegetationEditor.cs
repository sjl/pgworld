using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Vegetation))]
public class VegetationEditor : Editor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector();

		Vegetation vegetation = (Vegetation)target;

		if (GUILayout.Button("Run texturing")) {
			vegetation.RunTexturing();
		}

		if (GUILayout.Button("Run tree generator")) {
			vegetation.RunTreeGenerator();
		}

		if (GUILayout.Button("Remove trees")) {
			vegetation.RunRemoveTrees();
		}

		if (GUILayout.Button("Run rock generator")) {
			vegetation.RunRockGenerator();
		}
	}

}

[CustomEditor(typeof(BasicTextureVegetation))]
public class BasicTextureVegetationEditor : VegetationEditor {}

[CustomEditor(typeof(GrassPlacement))]
public class GrassPlacementEditor : Editor {
	public override void OnInspectorGUI () {
		DrawDefaultInspector();
		EditorGUILayout.HelpBox("Specify how many textures your script should affect. Then you specify for each texture the grass strength that it will generate (Default: 0 {no grass}).", MessageType.Info);
		GrassPlacement gr = (GrassPlacement) target;
		if (GUILayout.Button("Run Grass")) {
			gr.RunGrassGenerator();
		}

	}
}