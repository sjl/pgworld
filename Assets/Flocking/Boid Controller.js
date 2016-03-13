// these define the flock's behaviorvar minVelocity : float 	= 5;var maxVelocity : float 	= 20;
var randomness : float 		= 1; var flockSize : int			= 20;
var prefab : GameObject; 
var chasee : GameObject;

var flockCenter : Vector3;
var flockVelocity : Vector3;

private var boids;
function Start() {
	boids = new Array(flockSize);
	for (var i=0; i<flockSize; i++) {		var position = Vector3(						Random.value*GetComponent.<Collider>().bounds.size.x,						Random.value*GetComponent.<Collider>().bounds.size.y,						Random.value*GetComponent.<Collider>().bounds.size.z)-GetComponent.<Collider>().bounds.extents;		var boid = Instantiate(prefab, transform.position, transform.rotation);		boid.transform.parent = transform;		boid.transform.localPosition = position;
		boid.GetComponent("Boid Flocking").setController(gameObject);
		boids[i] = boid;
	}}function Update () {   
   	var theCenter = Vector3.zero;   	var theVelocity = Vector3.zero;
   	for (var boid : GameObject in boids) {
		theCenter       = theCenter + boid.transform.localPosition;		theVelocity     = theVelocity + boid.GetComponent.<Rigidbody>().velocity;
   	}
	flockCenter = theCenter/(flockSize);	
	flockVelocity = theVelocity/(flockSize);
}
