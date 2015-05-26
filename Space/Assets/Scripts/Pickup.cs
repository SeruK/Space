using UnityEngine;
using System.Collections;
using ItemType = Item.ItemType;

public class Pickup : MonoBehaviour {
	[SerializeField]
	private ItemType itemType;
	[SerializeField]
	private string localizedLineId;

	public ItemType ItemType {
		get { return itemType; }
	}

	public string LocalizedLineId {
		get  { return localizedLineId; }
	}
}
