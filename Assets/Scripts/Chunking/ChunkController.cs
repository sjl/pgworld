using UnityEngine;
using System;
using System.Collections;

struct ChunkCoord {
  public readonly int x;
  public readonly int z;
  public ChunkCoord(int chunkx, int chunkz) {
    x = chunkx;
    z = chunkz;
  }
}

public class ChunkController : MonoBehaviour {

	public GameObject player;
	public int chunkExponent = 6;
	public float chunkScale = 1;

	private Hashtable chunks;
	private int chunkResolution;
	private float chunkWidth;

	void Start () {
		chunks = new Hashtable();
		chunkResolution = (int)Math.Pow(2, chunkExponent) + 1;
		chunkWidth = chunkResolution * chunkScale;

		ensureChunk(new ChunkCoord(0, 0));
	}

	void ensureChunk(ChunkCoord c) {
		if (!chunks.Contains(c)) {
			chunks.Add(c, makeChunk(c));
		}
	}

	GameObject makeChunk(ChunkCoord c) {
		TerrainData tData = new TerrainData();

		tData.heightmapResolution = chunkResolution;
		tData.size = new Vector3(chunkWidth, chunkWidth, chunkWidth);

		GameObject terrain = Terrain.CreateTerrainGameObject(tData);
		terrain.transform.position = new Vector3(
				chunkWidth * c.x,
				0.0f,
				chunkWidth * c.z);

		return terrain;
	}

	void Update () {
		float px = player.transform.position.x;
		float pz = player.transform.position.z;

		int cx = (int) (px / chunkWidth);
		int cz = (int) (pz / chunkWidth);

		for (int dx = -3; dx <= 3; dx++) {
			for (int dz = -3; dz <= 3; dz++) {
				ensureChunk(new ChunkCoord(cx + dx, cz + dz));
			}
		}
	}
}
