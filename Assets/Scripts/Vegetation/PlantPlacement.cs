using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
public class PlantPlacement : MonoBehaviour
{
	// public Terrain terrain;
    public int plantType;
    public int[] texturesToAffect;

	public PlantPlacement ()
	{
	}

	public void RunPlantGenerator ()
	{
		// Get a reference to the terrain
		Terrain terrain = GetComponent<Terrain> ();

		// Get a reference to the terrain
		var terrainData = terrain.terrainData;

		int alphamapWidth = terrainData.alphamapWidth;
		int alphamapHeight = terrainData.alphamapHeight;
		int detailWidth = terrainData.detailResolution;
		int detailHeight = detailWidth;
       
		float resolutionDiffFactor = (float)alphamapWidth / detailWidth;
       
		float[,,] splatmap = terrain.terrainData.GetAlphamaps (0, 0, alphamapWidth, alphamapHeight);
       
		int[,] newDetailLayer = new int[detailWidth, detailHeight];

		var plantTypes = terrainData.detailPrototypes;
		if (plantType > plantTypes.Length || plantType < 0) {
			print ("Plant type not available");
			return;
		}

		//loop through splatTextures
		for (int i = 0; i < texturesToAffect.Length; i++) {
			//find where the texture is present
			for (int j = 0; j < detailWidth; j++) {  
				for (int k = 0; k < detailHeight; k++) {
					// alphaValue is how much of this texture is in this particular pixel
					float alphaValue = splatmap [(int)(resolutionDiffFactor * j), (int)(resolutionDiffFactor * k), i];

					var rnd = Random.Range (0, 1000);

					if (rnd == 0) {
						newDetailLayer[j, k] = (int)Mathf.Round(alphaValue * (float)texturesToAffect[i]) + newDetailLayer[j, k];
					}
                }
            }
        }
        terrainData.SetDetailLayer(0, 0, plantType, newDetailLayer); 
	}
}
