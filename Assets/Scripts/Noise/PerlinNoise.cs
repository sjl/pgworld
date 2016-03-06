using UnityEngine;
using System.Collections;

public class PerlinNoise : Noise {

	public float baseOctave = 0.25f;
	public float[] weights = {1.0f, 1.0f, 1.0f, 1.0f, 1.0f};
	private int poolSize = 6000;
	private Vector2[] pool;

	public override void Init() {
		pool = new Vector2[poolSize];
		for (int i = 0; i < pool.Length; i++) {
			pool[i] = new Vector2(Random.value, Random.value).normalized;
		}
	}

	private Vector2 GradientAt(int x, int y) {
		x = System.Math.Abs(x);
		y = System.Math.Abs(y);
		int index = Mathf.RoundToInt(0.5f*(x+y)*(x+y+1)+y) % poolSize;

		return pool[index];
	}

	private float Lerp(float a, float b, float w) {
		return a + (b - a)*w;
	}

	public override float Get(float x, float y) {
		float total = 0.0f;
		float Scale = baseOctave;
		foreach (float w in weights) {
			total += w * Raw(Scale*x, Scale*y);
			Scale *= 2.0f;
		}
		return total + 0.5f;
	}

	private float Raw(float x, float y) {
		Vector2 point = new Vector2(x, y);

		int x0 = Mathf.FloorToInt(x);
		int x1 = Mathf.CeilToInt(x);
		int y0 = Mathf.FloorToInt(y);
		int y1 = Mathf.CeilToInt(y);

		if (x1 == x0) {
			x1+=1;
		}
		if (y1 == y0) {
			y1+=1;
		}

		Vector2 p00 = new Vector2(x0,y0);
		Vector2 p10 = new Vector2(x1,y0);
		Vector2 p01 = new Vector2(x0,y1);
		Vector2 p11 = new Vector2(x1,y1);

		float n00 = Vector2.Dot(GradientAt(x0,y0), point-p00);
		float n10 = Vector2.Dot(GradientAt(x1,y0), point-p10);
		float n01 = Vector2.Dot(GradientAt(x0,y1), point-p01);
		float n11 = Vector2.Dot(GradientAt(x1,y1), point-p11);

		float wX = (x-x0)/(x1-x0);
		float wY = (y-y0)/(y1-y0);

		return Lerp (
			Lerp(n00, n10, wX),
			Lerp(n01, n11, wX),
			wY);
	}
		
}
