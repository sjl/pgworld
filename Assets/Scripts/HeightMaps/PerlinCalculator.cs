using UnityEngine;
using System.Collections;

public class PerlinCalculator {

	private int poolSize = 500;
	private Vector2[] pool;

	public PerlinCalculator() {
		pool = new Vector2[poolSize];
		for (int i = 0; i < pool.Length; i++) {
			pool[i] = new Vector2(Random.value, Random.value).normalized;
		}
	}

	private Vector2 GradientAt(int x, int y) {
		int index = Mathf.RoundToInt(0.5f*(x+y)*(x+y+1)+y) % poolSize;
		return pool[index];
	}

	private float Lerp(float a, float b, float w) {
		return a + (b - a)*w;
	}

	public float Get(float x, float z) {
		Vector2 point = new Vector2(x, z);

		int x0 = Mathf.FloorToInt(x);
		int x1 = Mathf.CeilToInt(x);
		int z0 = Mathf.FloorToInt(z);
		int z1 = Mathf.CeilToInt(z);

		if (x1 == x0) {
			x1+=1;
		}
		if (z1 == z0) {
			z1+=1;
		}

		Vector2 p00 = new Vector2(x0,z0);
		Vector2 p10 = new Vector2(x1,z0);
		Vector2 p01 = new Vector2(x0,z1);
		Vector2 p11 = new Vector2(x1,z1);

		float n00 = Vector2.Dot(GradientAt(x0,z0), point-p00);
		float n10 = Vector2.Dot(GradientAt(x1,z0), point-p10);
		float n01 = Vector2.Dot(GradientAt(x0,z1), point-p01);
		float n11 = Vector2.Dot(GradientAt(x1,z1), point-p11);

		float wX = x-x0/x1-x0;
		float wZ = z-z0/z1-z0;

		return Lerp (
			Lerp(n00, n10, wX),
			Lerp(n01, n11, wX),
			wZ);
	}
}
