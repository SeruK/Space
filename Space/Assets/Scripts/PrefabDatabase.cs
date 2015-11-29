using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PrefabDatabase : MonoBehaviour {
	[Serializable]
	public class Entry {
		public string Key;
		public GameObject Value;
	}

	[SerializeField]
	private Entry[] keyValuePairs;

	private Dictionary<string, GameObject> lookup;

	public GameObject this[ string name ] {
		get { return GetPrefabWithName( name ); }
	}

	public void Reinitialize() {
		lookup = keyValuePairs.ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
	}

	public GameObject GetPrefabWithName( string name ) {
		return lookup[ name ];
	}
}
