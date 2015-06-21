using UnityEngine;
using System.Collections;
using ItemType = Item.ItemType;

public class Pickup : BaseEntity {
	[SerializeField]
	private ItemType itemType;
	[SerializeField]
	private string localizedLineId;

	public ItemType ItemType {
		get { return itemType; }
		set { itemType = value; }
	}
	public string LocalizedLineId {
		get { return localizedLineId; }
		set { localizedLineId = value; }
	}
}
