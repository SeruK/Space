using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	public string DisplayName {
		get { return name; }
	}

	private string name = "none";
}
