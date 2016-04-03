using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class PGMath {
	public static float clamp(float value, float min, float max) {
		if (value < min) {
			return min;
		} else if (value > max) {
			return max;
		} else {
			return value;
		}
	}
	
	public static float expand(float value, float range) {
		// Expand the value that's expected to be within range of 0.5 to between
		// 0 and 1.
		return (value - (0.5f - range)) * (1.0f / (2 * range));
	}
}
