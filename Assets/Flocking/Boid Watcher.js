var boidController : Transform;

function LateUpdate () {
	if (boidController) {
		var watchPoint : Vector3 = boidController.GetComponent("Boid Controller").flockCenter;
		transform.LookAt(watchPoint+boidController.transform.position);	
	}
}