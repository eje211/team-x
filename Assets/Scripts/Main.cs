using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class Main : MonoBehaviour {
	
	public float speed = 1000f;
	
	private OSC_CS OSC;

	// Use this for initialization
	void Start () {
		GlobalData.SpawnSlots = Enumerable.Range(0, 10).ToList();
		OSC = GameObject.Find("OSC").GetComponent<OSC_CS>();
		GlobalData.speed = speed;
	}
	
	// Update is called once per frame
	// Move units around as per the instructions from the database.
	void Update () {
		// Make sure the cars behave at expected.
		foreach (KeyValuePair<string,GameObject> car in GlobalData.hashes) {
			if (car.Value.rigidbody.constraints == RigidbodyConstraints.None) continue;
			car.Value.transform.Rotate(new Vector3(- car.Value.transform.eulerAngles.x, 0, 0));
			car.Value.rigidbody.constraints = car.Value.rigidbody.constraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			car.Value.transform.position = new Vector3(car.Value.transform.position.x, 0.1f, car.Value.transform.position.z);
		}
		foreach(KeyValuePair<GameObject,Vector3> car in GlobalData.targets)
			if (!car.Key.GetComponent<CarElements>().hit)
				try {
					car.Key.transform.LookAt(car.Value);
				} catch(Exception e) {Debug.Log(e.ToString());
		}
		foreach (KeyValuePair<GameObject, Vector3> target in GlobalData.targets) {
			// Don't send message or otherwise take care of dead car.
			if (target.Key.GetComponent<CarLife>().dead) return;
			// Handle responses.
			OSC.Send("/locator", target.Key.name + ":" +
				Mathf.Round((target.Key.transform.position.x + 50) * 10).ToString() + "," +
				Mathf.Round((target.Key.transform.position.z + 50) * 10).ToString());
			// Debug.Log("Distance: " + Vector3.Distance(target.Key.transform.position, target.Value).ToString());
			if(Central.VerticalDistance(target.Key.transform.position, target.Value, 2.5f)) {
				target.Key.rigidbody.velocity = Vector3.zero;
				GlobalData.toRemove.Add(target.Key);
				continue;
			}
		}
		foreach (GameObject key in GlobalData.toRemove) {
			GlobalData.targets.Remove(key);
		}
		GlobalData.toRemove.Clear();
	}

}

public static class GlobalData {
	public static Dictionary<string, GameObject>  hashes      = new Dictionary<string, GameObject>();
	public static List<int>                       SpawnSlots  = new List<int>();
	public static List<string>                    waitingList = new List<string>();
	public static List<GameObject>                toRemove    = new List<GameObject>();
	public static Dictionary<GameObject, Vector3> targets     = new Dictionary<GameObject, Vector3>();
	public static float                           speed;
}
