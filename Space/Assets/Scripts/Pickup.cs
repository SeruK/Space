using UnityEngine;
using System.Collections;
using ItemType = Item.ItemType;

public class Pickup : MonoBehaviour {
	[SerializeField]
	private ItemType itemType;

	public ItemType ItemType {
		get { return itemType; }
	}

	public string DisplayName {
		get { return System.Enum.GetName( typeof(ItemType), itemType ); }
	}

	private string displayName = "none";
}
