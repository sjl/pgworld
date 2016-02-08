using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainErosion))]
public class TerrainErosionEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		TerrainErosion eroder = (TerrainErosion)target;
		if (GUILayout.Button("Run")) {
			eroder.Run();
		}
	}

}

// Dammit Unity...
[CustomEditor(typeof(NaiveTerrainErosion))]
public class NaiveTerrainErosionEditor : TerrainErosionEditor {}

[CustomEditor(typeof(ThermalTerrainErosion))]
public class ThermalTerrainErosionEditor : TerrainErosionEditor {}

[CustomEditor(typeof(HydraulicTerrainErosion))]
public class HydraulicTerrainErosionEditor : TerrainErosionEditor {}

