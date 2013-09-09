using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSC_controller_CS : MonoBehaviour {
	
	public OSC_CS oscMyServer;
	public GameObject prefab;
	
	void Update () {
		
	}
	
	/// <summary>
	/// Register handlers.
	/// </summary>
	void Start () {
		// start the server - receive addrs /buttons and /tuio/2dcur 
		oscMyServer.StartServer();
		
		// The touch info is automaticalyl handled by TUIO and used
		// to emulate an iDevice.
		// oscMyServer.SetHandler("/tuio/2Dcur", "TouchHandler")		// Get input from buttons on the station's panel
		oscMyServer.SetHandler("/color", "ColorHandler");
		
		// Input from the touch screen about touch data.
		oscMyServer.SetHandler("/quit", "QuitHandler");;
		
		// Input from the touch screen.
		oscMyServer.SetHandler("/touch", "TouchHandler");
		
		// Input from the touch screen.
		oscMyServer.SetHandler("/spawn", "SpawnHandler");
	}
	
	void OnDisable() {
		
	}
	
	/// <summary>
	/// Handles touch input from the board.
	/// </summary>
	/// <param name='m'>
	/// The hash of the car being moved, and its location in the format hash:x,y
	/// </param>
	void TouchHandler (string m) {
		Debug.Log("Got touch input.");
		Debug.Log("touch message: " + m); // Print the whole message.
	
		// Drop the destination now that we've used it.
		string message = m.Substring("/touch ".Length);
		
		// Only print the content.
		Debug.Log("touch message: " + message);
		// If the message comes from Panda on the local machine:
		
		
		string[] actionParams = message.Split(':');
		string hash = actionParams[0];
		actionParams = actionParams[1].Split(',');
		float x = - coordtrans(actionParams[0], "x");
		float y = coordtrans(actionParams[1], "y");

		// Get local representation of avatar
		GameObject avatar = GlobalData.hashes[hash];
		Dictionary<GameObject, Vector3> targets = GlobalData.targets;
		// Make a Vector3 for the coordinate.
		Vector3 target = new Vector3(x, 0.1f, y);
		// List it and apply it.
		if (targets.ContainsKey(avatar))
			targets[avatar] = target;
		else targets.Add(avatar, target);
		avatar.transform.LookAt(target);
		avatar.rigidbody.velocity = avatar.transform.forward.normalized * Time.deltaTime * GlobalData.speed;
		avatar.GetComponent<CarElements>().hit = false;

	}
	
	/// <summary>
	/// Translates the coordinates from the intermediate ones on the web server to the ones in Unity.
	/// </summary>
	/// <param name='dbcoord'>
	/// The intermediate coordinates.
	/// </param>
	/// <param name='axis'>
	/// Axis (no longer used).
	/// </param>
	private float coordtrans(string dbcoord, string axis) {
		float result;
		float floor = 100f; // (float) typeof(Vector3).GetProperty(axis).GetValue(GameObject.Find("Floor").transform.localScale, null);
		result = float.Parse(dbcoord);
		return (result / 1000f * floor) - (floor / 2);
	}
	
	/// <summary>
	/// Handles the action to destroy the car after the play has touched the "quit" button.
	/// </summary>
	/// <param name='m'>
	/// The hash of the car of to destroy.
	/// </param>
	public void QuitHandler (string m) {
		Debug.Log("Got destroy car request.");
		Debug.Log("destroy car message: " + m); // Print the whole message.
	
		// Drop the destination now that we've used it.
		string hash = m.Substring("/quit ".Length);
		MonoBehaviour.Destroy(GlobalData.hashes[hash]);
		GlobalData.hashes.Remove(hash);
	}
	
	
	/// <summary>
	/// Handle a request to change the color of a car.
	/// </summary>
	/// <param name='m'>
	/// The content of the message in the form hash:color . For example
	/// a8ba9ba0:red
	/// </param>
	void ColorHandler (string m) {
		Debug.Log("Got color change request.");
		Debug.Log("color message: " + m); // Print the whole message.
	
		// Drop the destination now that we've used it.
		string message = m.Substring("/color ".Length);
		string[] actionParams = message.Split(':');
		ColorChange(actionParams[0], actionParams[1]);
	}
	
	/// <summary>
	/// Handles the creation of a new car, when the user goes to the web page for the first time.
	/// </summary>
	/// <param name='m'>
	/// The hash of the car to create.
	/// </param>
	void SpawnHandler(string m) {
		Debug.Log("Got spawn request.");
		Debug.Log("spawn message: " + m); // Print the whole message.
	
		// Drop the destination now that we've used it.
		string hash = m.Substring("/spawn ".Length);
		// Do not add the car if we already have it!
		if (GlobalData.hashes.ContainsKey(hash)) Debug.Log("I know that player!");
		if (GlobalData.hashes.ContainsKey(hash)) return;
		// If there are no spawn slots, defer to next time.
		List<int> SpawnSlots = GlobalData.SpawnSlots;
		if (SpawnSlots.Count == 0) {
			GlobalData.waitingList.Add(hash);
			return;
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
		GlobalData.hashes.Add(newobject.name, newobject);
		ColorChange(newobject.name, "red");
		newobject.tag = "Car";
		newobject.GetComponent<CarElements>().spawnslot = spawnslot;
		// newobject.transform.Find("Start light"); // .animation.Play("Start light");
	}
	
	
	/// <summary>
	/// Changes the color of a car.
	/// </summary>
	/// <param name='hash'>
	/// The hash of the car to change.
	/// </param>
	/// <param name='colorName'>
	/// The name of the color to change to.
	/// </param>
	public static void ColorChange(string hash, string colorName) {
		GameObject avatar = GlobalData.hashes[hash];
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
	
//	void LocationHandler (string m) {
//		Debug.Log("Got location input.");
//		Debug.Log("message: " + m); // Print the whole message.
//		return;
//		GameObject.Find("GUI Text").guiText.text += m;
//	}
	
}
