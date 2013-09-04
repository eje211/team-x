using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

public class Main : MonoBehaviour {
	
	public GameObject prefab;
	public float speed = 1000f;
	
	public Dictionary<GameObject, Vector3> targets = new Dictionary<GameObject, Vector3>();
	private List<Connect.Action> itemsToDelete = new List<Connect.Action>();
	public List<int> SpawnSlots = new List<int>();

	// Use this for initialization
	void Start () {
		SpawnSlots = Enumerable.Range(0, 10).ToList();
		Central.mainClass = GameObject.Find("Main");
		Connect.ConnectToDB();
		Connect.ExecSql("DELETE FROM players");
		Connect.ExecSql("TRUNCATE todelete");
		Connect.ExecSql("TRUNCATE unityresponse");
		Thread SyncActions = new Thread(new ThreadStart(LookActions));
		SyncActions.Start();
	}
	
	// Update is called once per frame
	// Move units around as per the instructions from the database.
	void FixedUpdate () {
		SyncUnits();
		foreach(KeyValuePair<GameObject,Vector3> car in targets)
			if (!car.Key.GetComponent<CarElements>().hit)
				try {
					car.Key.transform.LookAt(car.Value);
				} catch(Exception e) {Debug.Log(e.ToString());}
		foreach (KeyValuePair<string,GameObject> car in Connect.hashes) {
			if (car.Value.rigidbody.constraints == RigidbodyConstraints.None) continue;
			car.Value.transform.Rotate(new Vector3(- car.Value.transform.eulerAngles.x, 0, 0));
			car.Value.rigidbody.constraints = car.Value.rigidbody.constraints | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			car.Value.transform.position = new Vector3(car.Value.transform.position.x, 0.1f, car.Value.transform.position.z);
		}
		List<GameObject> toRemove = new List<GameObject>();
		foreach (KeyValuePair<GameObject, Vector3> target in targets) {
			if (!Connect.hashes.ContainsValue(target.Key)) {
				toRemove.Add(target.Key);
				continue;
			}
			// Handle responses.
			Connect.ResponseSpooler(target.Key.name, "locator",
				Mathf.Round((target.Key.transform.position.x + 50) * 10).ToString() + "," +
				Mathf.Round((target.Key.transform.position.z + 50) * 10).ToString());
			// Debug.Log("Distance: " + Vector3.Distance(target.Key.transform.position, target.Value).ToString());
			if(Central.VerticalDistance(target.Key.transform.position, target.Value, 2.5f)) {
				target.Key.rigidbody.velocity = Vector3.zero;
				toRemove.Add(target.Key);
				continue;
			}
		}
		foreach (GameObject idleCar in toRemove)
			targets.Remove(idleCar);
		HandleActions();
	}
	
	public void LookActions() {
		while(true) {
			Thread.Sleep(1000);
			Connect.LoadPlayers();
			Connect.LoadActions();
			Connect.Respond();
		}
	}
	
	public void HandleActions() {
		// Handle listed actions.
		if (Connect.pendingActions.Count == 0)
			return;
		
		List<Connect.Action> LocalCopy = Connect.pendingActions;
		Connect.pendingActions = new List<Connect.Action>();
		
		foreach (Connect.Action action in LocalCopy) {
			/* if (!Connect.hashes.ContainsKey(action.hash)) {
				itemsToDelete.Add(action);
				continue;
			} */
			switch (action.type) {
			case "color":
				string[] actionParams = action.action.Split('=');
				ColorChange(action.hash, actionParams[1]);
				break;
			case "locator":
				// Get local representation of avatar
				GameObject avatar = Connect.hashes[action.hash];
				// Make a Vector3 for the coordinate.
				string[] coordinates = action.action.Split('&');
				float x = - coordtrans(coordinates[0], "x");
				float y = coordtrans(coordinates[1], "y");
				Vector3 target = new Vector3(x, 0.1f, y);
				// List it and apply it.
				if (targets.ContainsKey(avatar))
					targets[avatar] = target;
				else targets.Add(avatar, target);
				avatar.transform.LookAt(target);
				avatar.rigidbody.velocity = avatar.transform.forward.normalized * Time.deltaTime * speed;
				avatar.GetComponent<CarElements>().hit = false;
				break;
			}
		}
		if (itemsToDelete.Count == 0)
			return;
		
		foreach (Connect.Action item in itemsToDelete)
			Connect.pendingActions.Remove(item);
		itemsToDelete = new List<Connect.Action>();
	}
	
	private float coordtrans(string dbcoord, string axis) {
		float result;
		float floor = 100f; // (float) typeof(Vector3).GetProperty(axis).GetValue(GameObject.Find("Floor").transform.localScale, null);
		result = float.Parse(dbcoord.Split('=')[1]);
		return (result / 1000f * floor) - (floor / 2);
	}
	
	/**
	 * Synchronize cars in Unity with the data in the database.
	 */ 
	public void SyncUnits() {
		List<string> LocalCopy = Connect.UnitsToCreate;
		Connect.UnitsToCreate = new List<string>();
		foreach (string hash in LocalCopy) {
			// If there are no spawn slots, defer to next time.
			if (SpawnSlots.Count == 0) {
				Connect.UnitsToCreate.Add(hash);
				continue;
			}
			// Find a spawn slot.
			// Get index of a random AVAILABLE spawn spot.
			int spawnindex = UnityEngine.Random.Range(1, SpawnSlots.Count);
			// Get actual slot.
			int spawnslot = SpawnSlots[spawnindex];
			SpawnSlots.Remove(spawnslot);
			float spawnlocation = -45f + (float) (spawnslot * 10) ;
			GameObject newobject = (GameObject) Instantiate(prefab, new Vector3(spawnlocation, 0.101f, -55f), Quaternion.identity);
			newobject.name = hash;
			Connect.hashes.Add(newobject.name, newobject);
			ColorChange(newobject.name, "red");
			newobject.tag = "Car";
			newobject.GetComponent<CarElements>().spawnslot = spawnslot;
		}
		LocalCopy = Connect.hashesToDelete;
		Connect.hashesToDelete = new List<string>();
		foreach (string hashToDelete in LocalCopy) {
			if (targets.ContainsKey(Connect.hashes[hashToDelete]))
			    targets.Remove(Connect.hashes[hashToDelete]);
	
			MonoBehaviour.Destroy(Connect.hashes[hashToDelete]);
			Connect.hashes.Remove(hashToDelete);
		}
	}
	
	public static void ColorChange(string hash, string colorName) {
		GameObject avatar = Connect.hashes[hash];
		// avatar.renderer.material.mainTexture = (Texture) typeof(CarElements).GetProperty(colorName + "_color").GetValue(avatar.GetComponent<CarElements>(), null);
		
		switch (colorName) {
		case "red":
			avatar.renderer.material.mainTexture = avatar.GetComponent<CarElements>().red_color;
			break;
		case "blue":
			avatar.renderer.material.mainTexture = avatar.GetComponent<CarElements>().blue_color;
			break;
		case "green":
			avatar.renderer.material.mainTexture = avatar.GetComponent<CarElements>().green_color;
			break;
		case "yellow":
			avatar.renderer.material.mainTexture = avatar.GetComponent<CarElements>().yellow_color;
			break;
		case "orange":
			avatar.renderer.material.mainTexture = avatar.GetComponent<CarElements>().orange_color;
			break;
		case "purple":
			avatar.renderer.material.mainTexture = avatar.GetComponent<CarElements>().purple_color;
			break;
		}
		}
}


