using UnityEngine;
using System.Collections;

public class SpawnLight : MonoBehaviour {
	
	public Shader diffuse;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void Suicide () {
		this.transform.parent.gameObject.renderer.material.shader = diffuse;
		this.gameObject.SetActive(false);
	}
}
