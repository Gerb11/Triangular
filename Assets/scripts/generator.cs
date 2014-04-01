using UnityEngine;
using System.Collections;

/**
 * For shooting the triangles. Sets a bool so that only one triangle can be fired at once,
 * Applys the necessary forces of the triangle.
 */
public class generator : MonoBehaviour {

	public GameObject prefab;
	public GameObject shooter;
	public float shootingForce;

	private GameObject queue;
	private queue queueScript;

	void Awake() {
		queue = GameObject.Find ("Queue");
		queueScript = queue.GetComponent<queue> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
 		GameObject cannon = GameObject.FindGameObjectWithTag("Player");
		
		if(Input.GetMouseButtonUp(0) && GlobalFlags.canFire)
		{
			GameObject triInstance = queueScript.fireShot();//getting the triangle from the queue, so the queue can update
			queueScript.randomLevelQueue();
			if(triInstance != null) {
				triInstance.transform.localPosition = shooter.transform.position + ((shooter.transform.rotation * Vector3.up) * 0.3f);
				triInstance.transform.localRotation = Quaternion.identity;

				triInstance.rigidbody.AddForce((shooter.transform.rotation * Vector3.up) * shootingForce);
			
				Vector3 rot = transform.rotation.eulerAngles;
				rot.z -= 60;
				triInstance.transform.rotation = Quaternion.Euler(rot);
			
				cannon.transform.localRotation = Quaternion.Euler(new Vector3(0,0,180));
		if (!GlobalFlags.getPaused()){
	 		GameObject cannon = GameObject.FindGameObjectWithTag("Player");
			
				GlobalFlags.canFire = false;
				GlobalFlags.trianglesStatic = false;
			}
		}
	}
}
