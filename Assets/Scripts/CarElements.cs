using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CarElements : MonoBehaviour {
	
	public Texture    red_color;
	public Texture   blue_color;
	public Texture  green_color;
	public Texture yellow_color;
	public Texture purple_color;
	public Texture orange_color;
	public int        spawnslot;
	public bool             hit;
	public int         hitcount = 0;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter(Collision collision) {
		if (collider.gameObject.tag == "Car" && collider.gameObject.transform != transform) {
			hit = true;
			collider.gameObject.GetComponent<CarElements>().hitcount++;
			Debug.Log(String.Format("{0} count: {1} -- {2} count : {3}",
				collider.gameObject.name,
				collider.gameObject.GetComponent<CarElements>().hitcount,
				name, hitcount));
		}
	}
}
