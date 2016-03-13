using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidFlocking : MonoBehaviour
{
	internal BoidController controller;

	IEnumerator Start()
	{
		while (true)
		{
			if (controller)
			{
				GetComponent<Rigidbody>().velocity += steer() * Time.deltaTime;

				// enforce minimum and maximum speeds for the boids
				float speed = GetComponent<Rigidbody>().velocity.magnitude;
				if (speed > controller.maxVelocity)
				{
					GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * controller.maxVelocity;
				}
				else if (speed < controller.minVelocity)
				{
					GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * controller.minVelocity;
				}
			}
			float waitTime = Random.Range(0.3f, 0.5f);
			yield return new WaitForSeconds(waitTime);
		}
	}

	Vector3 steer()
	{
		Vector3 randomize = new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1);
		randomize.Normalize();
		randomize *= controller.randomness;

		Vector3 center = controller.flockCenter - transform.localPosition;
		Vector3 velocity = controller.flockVelocity - GetComponent<Rigidbody>().velocity;
		Vector3 follow = controller.target.localPosition - transform.localPosition;

		return (center + velocity + follow * 2 + randomize);
	}
}