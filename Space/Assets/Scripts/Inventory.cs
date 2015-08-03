using UnityEngine;
using System.Collections;
using ItemType = Item.ItemType;
using SA;

public struct InventoryItem {
	public static InventoryItem None = new InventoryItem();

	public readonly ItemType ItemType;
	public short Amount;

	public InventoryItem( ItemType itemType, short amount ) {
		this.ItemType = itemType;
		this.Amount = amount;
	}
}

public class Inventory : MonoBehaviour {
	[HideInInspector]
	public Sprite[] sprites;

	public int Height {
		get { return items.Rank; }
	}

	public int Width {
		get { return items.GetLength( 0 ); }
	}

	private InventoryItem[,] items = new InventoryItem[ 5,5 ];

	public InventoryItem ItemAt( int x, int y ) {
		return items[ y, x ];
	}

	public bool AddItem( ItemType item, short amount ) {
		if( item == ItemType.None ) {
			return false;
		}

		for( int y = 0; y < items.Length; ++y ) {
			for( int x = 0; x < items.GetLength( y ); ++x ) {
				InventoryItem invItem = items[ y, x ];
				if( invItem.ItemType == item && ( (int)invItem.Amount + (int)amount ) < Item.StackAmount( invItem.ItemType ) ) {
					items[ y, x ] = new InventoryItem( item, (short)( invItem.Amount + amount ) );
					return true;
				} else if( invItem.ItemType == ItemType.None ) {
					items[ y, x ] = new InventoryItem( item, amount );
					return true;
				}
			}
		}

		return false;
	}

	public void RemoveSingleItem( Vector2i pos ) {
		InventoryItem item = items[ pos.y, pos.x ];
		if( item.ItemType == ItemType.None ) {
			return;
		}
		if( item.Amount - 1 <= 0 ) {
			items[ pos.y, pos.x ] = InventoryItem.None;
		} else {
			items[ pos.y, pos.x ] = new InventoryItem( item.ItemType, (short)( item.Amount - 1 ) );
		}
	}

	// TODO: Better way for this
	public Sprite GetItemSprite( ItemType item, TilesetLookup tilesetLookup ) {
		if( Item.IsTile( item ) ) {
			return tilesetLookup.Tiles[ (int)Item.TileUUIDFromItem( item ) ].TileSprite;
		}

		return sprites[ (int)item ];
	}
}
