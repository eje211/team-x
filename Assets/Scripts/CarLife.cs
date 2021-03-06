﻿using UnityEngine;
using System.Collections;

public class CarLife : MonoBehaviour {
	
	public Detonator explosion;
	public bool dead = false;
	private OSC_CS OSC;

	// Use this for initialization
	void Start () {
		OSC = GameObject.Find("OSC").GetComponent<OSC_CS>();
	}
	
	// Update is called once per frame
	void Update () {
		if (dead) return;
		if (transform.position.y < 0) {
			dead = true;
			StartCoroutine("Explode");
		}
	}
	
	IEnumerator Explode() {
		yield return new WaitForSeconds(0.75f);
		GlobalData.targets.Remove(gameObject);
		GlobalData.hashes.Remove(name);
		OSC.Send("/crashed", name);
		Destroy(gameObject);
		Detonator boom = (Detonator) Instantiate(explosion, transform.position, Quaternion.identity);
		boom.Explode();

	}
}
