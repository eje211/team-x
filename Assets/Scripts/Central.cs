using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Central{

	public static GameObject mainClass = GameObject.Find("Main");
	public static List<GameObject> moving = new List<GameObject>();
	public static float speed = 1000f;
	
	public static bool VerticalDistance(Vector3 one, Vector3 two, float distance) {
		bool result;
		result = (Vector3.Distance(new Vector3(one.x, 0f, one.z),
			new Vector3(two.x, 0, two.z)) <= distance);
		return result;
	}

}
