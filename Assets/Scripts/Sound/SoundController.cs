using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SoundController : MonoBehaviour {
	public GameObject player;

	public AudioSource wind;

	private PerlinNoise windNoise;
	public float noiseBaseOctave = 0.5f;
	public float[] noiseWeights = {
		100.0f, 1.0f
	};
	public float windBlendRate = 0.1f;

	public Text windVolumeUI;
	public GameObject chunkController;

	private float underwaterLevel = 0.0f;

	void Start () {
		windNoise = new PerlinNoise();
		windNoise.baseOctave = noiseBaseOctave;
		windNoise.weights = noiseWeights;
		windNoise.Init();

		underwaterLevel =
			(chunkController.GetComponent<ChunkController>().terrainHeight)
			* (chunkController.GetComponent<ChunkController>().waterHeight);
	}

	private float clamp(float value, float min, float max) {
		if (value < min) {
			return min;
		} else if (value > max) {
			return max;
		} else {
			return value;
		}
	}
	
	private float expand(float value) {
		return (value - 0.40f) * 5;
	}

	void Update () {
		float py = player.transform.position.y;
		float vol = windNoise.Get(Time.fixedTime * windBlendRate, 0.0f);

		float windVolume = clamp(expand(vol), 0.1f, 1.0f);

		wind.volume = windVolume;
		windVolumeUI.text = "Wind blend: " + windVolume.ToString();

		if ((py + 1.1f) < underwaterLevel) {
			wind.volume = 0.0f;
		}
	}
}
