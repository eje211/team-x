using UnityEngine;
using System.Collections;

public class WallTriggers : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnTriggerExit(Collider collider) {
		if (collider.gameObject.tag == "Car" && collider.gameObject.transform != transform) 
			collider.gameObject.rigidbody.constraints = RigidbodyConstraints.None;
	}
}
