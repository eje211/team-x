using UnityEngine;
using System.Collections;

public class Testing : MonoBehaviour {
	
	public float speed = 1000f;
	
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if((transform.position - GameObject.Find("Three").transform.position).magnitude < 1f) speed = 0f;
		float angle;
		Vector3 axis;
		transform.LookAt(GameObject.Find("Three").transform.position);
		GameObject.Find("Three").transform.rotation.ToAngleAxis(out angle, out axis);
		rigidbody.velocity = transform.forward * Time.deltaTime * speed;
	}
	
	Vector3 rotate(Vector3 coord) {
		Vector3 result = new Vector3(-90f, coord.z, coord.x);
		return result;
	}
}
