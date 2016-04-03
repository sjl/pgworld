using UnityEngine;
using System.Collections;

public class UnderWater : MonoBehaviour {
 
	//This script enables underwater effects. Attach to player camera.

    public Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	public Color deepColor = new Color(0.0f, 0.0f, 0.0f, 0.8f);
	public Color underwaterColor = new Color(0, 0.4f, 0.7f, 0.1f);
	public GameObject waterplane = null;
	public GameObject projector = null;

	private Vector3 above, below, swimUp, sink;
	private bool isUnderwater;
	public GameObject chunk;

	private float underwaterLevel = 0.0f;
	private float calculatedDeepLevel = 0.0f;
	public float deepLevel = 0.0f;

	void Start() {
		underwaterLevel = (chunk.GetComponent<ChunkController>().terrainHeight) * (chunk.GetComponent<ChunkController>().waterHeight);
		calculatedDeepLevel = underwaterLevel - deepLevel;
		projector.transform.position = new Vector3(transform.position.x, underwaterLevel, transform.position.z);
		above = new Vector3(0.0f, 0.0f, 0.0f);
		below = new Vector3(180.0f, 0.0f, 0.0f);
		if (RenderSettings.fog == false) {
		RenderSettings.fog = true;
		}
	}

    void Update() {
    	waterplane.transform.position = new Vector3(transform.position.x, underwaterLevel, transform.position.z);
		
		if ((transform.position.y < underwaterLevel) != isUnderwater)
			isUnderwater = transform.position.y < underwaterLevel;
		if (isUnderwater) {
			SetUnderwater();
		} else if (!isUnderwater) {
			SetNormal();
		}
	}

	void SetUnderwater() {
		float depth = underwaterLevel - transform.position.y;
		float percentDepth = depth / (underwaterLevel - calculatedDeepLevel);
		if (percentDepth > 1.0f) {
			percentDepth = 1.0f;
		}
		RenderSettings.fogColor = Color.Lerp(underwaterColor, deepColor, percentDepth);
		RenderSettings.fogDensity = Mathf.Lerp(0.005f, 0.5f, percentDepth);
		waterplane.transform.localEulerAngles = below;
    }
    void SetNormal() {
    	RenderSettings.fogColor = normalColor;
		RenderSettings.fogDensity = 0.000000008f;
		waterplane.transform.localEulerAngles = above;
	}
}
