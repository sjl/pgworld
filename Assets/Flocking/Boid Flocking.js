var Controller : GameObject;

private var inited = false;
private var minVelocity : float;private var maxVelocity : float;
private var randomness : float;
private var chasee : GameObject;

private var clumping : float;
//private var 

function Start () {
	StartCoroutine("boidSteering");	
}

function boidSteering () {
	while(true) {
		if (inited) {
			GetComponent.<Rigidbody>().velocity = GetComponent.<Rigidbody>().velocity + calc() * Time.deltaTime;						// enforce minimum and maximum speeds for the boids			var speed = GetComponent.<Rigidbody>().velocity.magnitude;			if (speed > maxVelocity) {				GetComponent.<Rigidbody>().velocity = GetComponent.<Rigidbody>().velocity.normalized * maxVelocity;
			} else if (speed < minVelocity) {				GetComponent.<Rigidbody>().velocity = GetComponent.<Rigidbody>().velocity.normalized * minVelocity;
			}
		}
	waitTime = Random.Range(0.3, 0.5);
	yield WaitForSeconds(waitTime);
	}
}

function calc () {
	var randomize 	= Vector3((Random.value *2) -1, (Random.value * 2) -1, (Random.value * 2) -1);
	
	randomize.Normalize();
	randomize *= randomness;
		
	flockCenter = Controller.GetComponent("Boid Controller").flockCenter; 
	flockVelocity = Controller.GetComponent("Boid Controller").flockVelocity;
	follow = chasee.transform.localPosition;
	
	flockCenter = flockCenter - transform.localPosition;
	flockVelocity = flockVelocity - GetComponent.<Rigidbody>().velocity;
	follow = follow - transform.localPosition;
		
	return (flockCenter+flockVelocity+follow*2+randomize);
}

function setController (theController : GameObject) {
		Controller = theController;
		minVelocity = Controller.GetComponent("Boid Controller").minVelocity;
		maxVelocity = Controller.GetComponent("Boid Controller").maxVelocity;
		randomness 	= Controller.GetComponent("Boid Controller").randomness;
		chasee 		= Controller.GetComponent("Boid Controller").chasee;
		inited 		= true;
}