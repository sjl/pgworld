using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SoundController : MonoBehaviour {
	public GameObject player;
	public GameObject chunkController;

	// Audio sources and base volumes
	public AudioSource lightWind;
	public float lightWindBaseVolume = 1.0f;

	public AudioSource heavyWind;
	public float heavyWindBaseVolume = 1.0f;

	public AudioSource underWater;
	public float underWaterBaseVolume = 1.0f;

	public AudioSource birds;
	public float birdsBaseVolume = 1.0f;

	// Perlin Noise for the wind damping
	private PerlinNoise windNoise;
	public float windNoiseBaseOctave = 0.5f;
	public float[] windNoiseWeights = {
		100.0f, 1.0f
	};
	public float windBlendRate = 0.1f;

	// Perlin noise for the bird damping
	private PerlinNoise birdNoise;
	public float birdNoiseBaseOctave = 0.5f;
	public float[] birdNoiseWeights = {
		100.0f, 1.0f
	};
	public float birdBlendRate = 0.1f;

	// Cutoffs
	private float heavyWindHeight;
	private float underwaterLevel;

	// UI
	public Text windVolumeUI;

	void Start () {
		windNoise = new PerlinNoise();
		windNoise.baseOctave = windNoiseBaseOctave;
		windNoise.weights = windNoiseWeights;
		windNoise.Init();

		birdNoise = new PerlinNoise();
		birdNoise.baseOctave = birdNoiseBaseOctave;
		birdNoise.weights = birdNoiseWeights;
		birdNoise.Init();

		// Retrieve some settings from the chunk controller.
		ChunkController chunk = chunkController.GetComponent<ChunkController>();

		heavyWindHeight = chunk.terrainHeight * chunk.mountainPeekHeight * 1.2f;
		underwaterLevel = chunk.terrainHeight * chunk.waterHeight;
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
	
	private float expand(float value, float range) {
		// Expand the value that's expected to be within range of 0.5 to between
		// 0 and 1.
		return (value - (0.5f - range)) * (1.0f / (2 * range));
	}

	void Update () {
		float py = player.transform.position.y;

		float vol = windNoise.Get(Time.fixedTime * windBlendRate, 0.0f);
		float windVolume = clamp(expand(vol, 0.1f), 0.1f, 1.0f);

		vol = birdNoise.Get(Time.fixedTime * birdBlendRate, 0.0f);
		float birdVolume = clamp(expand(vol, 0.1f), 0.1f, 1.0f);

		if (py < underwaterLevel) {
			lightWind.volume = 0.0f;
			heavyWind.volume = 0.0f;
			underWater.volume = 1.0f;
			birds.volume = 0.0f;
		} else if (py > heavyWindHeight) {
			lightWind.volume = 0.0f;
			heavyWind.volume = 1.0f;
			underWater.volume = 0.0f;
			birds.volume = 0.0f;
		} else {
			float heavyWindWeight = (py - underwaterLevel) / (heavyWindHeight - underwaterLevel);
			float lightWindWeight = (1.0f - heavyWindWeight);
			lightWind.volume = lightWindWeight;
			heavyWind.volume = heavyWindWeight;
			underWater.volume = 0.0f;

			// Birds are only found at the lower elevations.
			birds.volume = birdVolume * (lightWindWeight * lightWindWeight);
		}

		// Also damp the (light) wind over time to let it ebb and flow.
		lightWind.volume *= windVolume;

		// Adjust the overall levels.
		lightWind.volume *= lightWindBaseVolume;
		underWater.volume *= underWaterBaseVolume;
		heavyWind.volume *= heavyWindBaseVolume;
		birds.volume *= birdsBaseVolume;

		windVolumeUI.text = "Wind volume: " + windVolume.ToString();
		windVolumeUI.text += " / Bird volume: " + birdVolume.ToString();
	}
}
