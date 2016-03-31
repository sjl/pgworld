using UnityEngine;
using System.Collections;
using UnityEditor;

public class Grasser : MonoBehaviour
{
	// public Terrain terrain;
    public int grassType;
    public int[] texturesToAffect;

	public Grasser ()
	{
	}

	public void RunGrassGenerator () {
		// Get a reference to the terrain
		Terrain terrain = GetComponent<Terrain>();

		// Get a reference to the terrain
		var terrainData = terrain.terrainData;

        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;
        int detailWidth = terrainData.detailResolution;
        int detailHeight = detailWidth;

        print(detailWidth);
		print(terrainData.alphamapResolution);
        print(alphamapWidth);
        print(alphamapHeight);
       
        float[,,] splatmap = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
		int[,] newDetailLayer = new int[detailWidth, detailHeight];
		float resolutionDiffFactor = (float)splatmap.GetLength(2) / newDetailLayer.GetLength(0);

		print(resolutionDiffFactor);
       
		var grassTypes = terrain.terrainData.detailPrototypes;
		if (grassType > grassTypes.Length || grassType < 0) {
			print("Grass type not available");
			return;
		}

        //loop through splatTextures
        for (int i = 0; i < texturesToAffect.Length; i++) {
            //find where the texture is present
			for (int j = 0; j < newDetailLayer.GetLength(1); j++) {  
				for (int k = 0; k < newDetailLayer.GetLength(0); k++) {
                	// alphaValue is how much of this texture is in this particular pixel
                    float alphaValue = splatmap[(int)(resolutionDiffFactor * j), (int)(resolutionDiffFactor * k), i];

					newDetailLayer[j, k] = (int)Mathf.Round(alphaValue * (float)texturesToAffect[i]) + newDetailLayer[j, k];
                }
            }
        }
        terrainData.SetDetailLayer(0, 0, grassType, newDetailLayer); // 

		/*print(terrainData.detailPrototypes[0].prototypeTexture); // GrassFrond02AlbedoAlpha (UnityEngine.Texture2D)
		print(terrainData.detailPrototypes[0].bendFactor); // 0.1
		print(terrainData.detailPrototypes[0].dryColor); // RGBA(0.804, 0.737, 0.102, 1.000)
		print(terrainData.detailPrototypes[0].healthyColor); // RGBA(0.263, 0.976, 0.165, 1.000)
		print(terrainData.detailPrototypes[0].maxHeight); // 0.5
		print(terrainData.detailPrototypes[0].minHeight); // 0.2
		print(terrainData.detailPrototypes[0].maxWidth); // 1
		print(terrainData.detailPrototypes[0].minWidth); // 0.5
		print(terrainData.detailPrototypes[0].noiseSpread); // 0.1
		print(terrainData.detailPrototypes[0].prototype); // Null
		print(terrainData.detailPrototypes[0].renderMode); // GrassBillboard
		print(terrainData.detailPrototypes[0].usePrototypeMesh); // false*/
	}
}
