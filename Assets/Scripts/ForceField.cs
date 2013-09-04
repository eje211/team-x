using UnityEngine;
using System.Collections;

public class ForceField : MonoBehaviour {
	
	void OnCollisionExit(Collision collision) {
		int spawnslot = collision.gameObject.GetComponent<CarElements>().spawnslot;
		Central.mainClass.GetComponent<Main>().SpawnSlots.Add(spawnslot);
		
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
