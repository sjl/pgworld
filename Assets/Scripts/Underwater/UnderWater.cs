using UnityEngine;
using System.Collections;
 
public class UnderWater : MonoBehaviour {
 
	//This script enables underwater effects. Attach to player camera.

    public float underwaterLevel = 0;
	public float deepLevel = 50;
	private bool isUnderwater;
	public Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	public Color deepColor = new Color(0.0f, 0.0f, 0.0f, 0.8f);
	public Color underwaterColor = new Color(0, 0.4f, 0.7f, 0.1f);
	private Vector3 above, below, swimUp, sink;
	public GameObject waterplane = null;
	public GameObject projector = null;

	void Start() {
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
		float percentDepth = depth / deepLevel;
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