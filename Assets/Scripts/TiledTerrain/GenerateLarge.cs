using UnityEngine;
using System.Collections;

public class GenerateLarge : MonoBehaviour {

	public GameObject plane;
	public GameObject player;
	public int planeSize = 10;
	public int halfTilesX = 10;
	public int halfTilesZ = 10;

	Vector3 startPos;

	Hashtable tiles = new Hashtable();

	void Start() {
		this.gameObject.transform.position = Vector3.zero;
		startPos = Vector3.zero;

		float updateTime = Time.realtimeSinceStartup;

		for (int x = -halfTilesX; x < halfTilesX; x++) {
			for (int z = -halfTilesZ; z < halfTilesZ; z++) {
				Vector3 pos = new Vector3((x * planeSize+startPos.x), 0, (z * planeSize+startPos.z));
				GameObject t = (GameObject) Instantiate(plane, pos, Quaternion.identity);

				string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();
				t.name = tilename;
				Tile tile = new Tile(t, updateTime);
				tiles.Add(tilename, tile);
			}
		}
	}
}
