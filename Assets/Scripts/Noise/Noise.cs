using UnityEngine;
using System.Collections;

public abstract class Noise : MonoBehaviour {

	public abstract float Get(float x, float y);
	public abstract void Init();
}
