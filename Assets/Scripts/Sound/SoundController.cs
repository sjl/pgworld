using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SoundController : MonoBehaviour {
	public GameObject player;

	public AudioSource wind;

	private PerlinNoise windNoise;
	public float noiseBaseOctave = 0.000003f;
	public float[] noiseWeights = {
		1.0f, 0.01f
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

	private float clamp(float value) {
		if (value < 0.0f) {
			return 0.0f;
		} else if (value > 1.0f) {
			return 1.0f;
		} else {
			return value;
		}
	}
	
	private float expand(float value) {
		return (value - 0.45f) * 10;
	}

	void Update () {
		float py = player.transform.position.y;
		float vol = windNoise.Get(Time.fixedTime * windBlendRate, 0.0f);

		float windVolume = clamp(expand(vol));

		wind.volume = windVolume;
		windVolumeUI.text = "Wind blend: " + windVolume.ToString();

		if ((py + 1.1f) < underwaterLevel) {
			wind.volume = 0.0f;
		}
	}
}
