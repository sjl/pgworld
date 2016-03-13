using UnityEngine;
using System.Collections;

public class Tile {

	public GameObject theTile;
	public float creationTime;

	public Tile() {
	}

	public Tile(GameObject t) {
		theTile = t;
	}

	public Tile(GameObject t, float ct) {
		theTile = t;
		creationTime = ct;
	}
}
