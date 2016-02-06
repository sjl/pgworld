using UnityEngine;
using System.Collections;

public abstract class Erosion : MonoBehaviour {

	public abstract Vector3[] Erode(Vector3[] current, int sizeX, int sizeZ);
}
