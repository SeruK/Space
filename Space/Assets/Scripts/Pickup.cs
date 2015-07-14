using UnityEngine;
using System.Collections;
using ItemType = Item.ItemType;

public class Pickup : BaseEntity {
	[SerializeField]
	private ItemType itemType;
	[SerializeField]
	private System.UInt32 tileUUID;
	[SerializeField]
	private string localizedLineId;

	public ItemType ItemType {
		get { return itemType; }
		set { itemType = value; }
	}
	public System.UInt32 TileUUID {
		get { return tileUUID; }
		set { tileUUID = value; }
	}
	public string LocalizedLineId {
		get { return localizedLineId; }
		set { localizedLineId = value; }
	}
	public bool Pickupable {
		get { return noPickupTimer <= 0.0f; }
	}

	public float noPickupTimer;

	protected void Update() {
		if( noPickupTimer > 0.0f ) {
			noPickupTimer -= Time.deltaTime;
		}
	}
}
