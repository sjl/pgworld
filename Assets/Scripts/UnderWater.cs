using UnityEngine;
using System.Collections;
 
public class UnderWater : MonoBehaviour {
 
	//This script enables underwater effects. Attach to player.

    public float underwaterLevel = 0;
	private bool isUnderwater, canSwim;
	public Color underwaterColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
	private Color normalColor = new Color(0, 0.4f, 0.7f, 0.6f);
	private Vector3 above, below, swimUp, sink;
	public GameObject waterplane = null;
	public GameObject controller = null;
	private ConstantForce force;
	private Rigidbody body;

	void Start() {
		above = new Vector3(0.0f, 0.0f, 0.0f);
		below = new Vector3(180.0f, 0.0f, 0.0f);
		swimUp = new Vector3(0.0f, 10.0f, 0.0f);
		sink = new Vector3(0.0f, -2.0f, 0.0f);
		body = controller.GetComponent<Rigidbody>();
		force = controller.GetComponent<ConstantForce>();
	}

    void Update() {
		if ((transform.position.y < underwaterLevel) != isUnderwater)
			isUnderwater = transform.position.y < underwaterLevel;
		if (isUnderwater) {
			SetUnderwater();
		} else if (!isUnderwater) {
			SetNormal();
		}

		if (canSwim) {
			if (Input.GetKey(KeyCode.E)) {
				force.relativeForce = swimUp;
			} else {
				force.relativeForce = sink;
			}
		}
	}

	void SetUnderwater() {
        RenderSettings.fogColor = normalColor;
        RenderSettings.fogDensity = 0.1f;
		waterplane.transform.localEulerAngles = below;
		body.useGravity = false;
		canSwim = true;
		//body.isKinematic = true;
    }
    void SetNormal() {
    	RenderSettings.fogColor = underwaterColor;
		RenderSettings.fogDensity = 0.008f;
		waterplane.transform.localEulerAngles = above;
		body.useGravity = true;
		canSwim = false;
		//body.isKinematic = false;
	}
}