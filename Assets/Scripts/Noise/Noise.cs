using UnityEngine;
using System.Collections;

public abstract class Noise {

	public abstract float Get(float x, float y);
	public abstract void Init();
}
