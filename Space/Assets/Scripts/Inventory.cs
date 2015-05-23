using UnityEngine;
using System.Collections;
using ItemType = Item.ItemType;

public class Inventory : MonoBehaviour {
	[HideInInspector]
	public Sprite[] sprites;

	private ItemType[,] items = new ItemType[ 5,5 ];

	public ItemType[,] Items {
		get {
			return items;
		}
	}

	public bool AddItem( ItemType item ) {
		if( item == ItemType.None ) {
			return false;
		}

		for( int y = 0; y < items.Length; ++y ) {
			for( int x = 0; x < items.GetLength( y ); ++x ) {
				if( items[y,x] == ItemType.None ) {
					items[y,x] = item;
					return true;
				}
			}
		}

		return false;
	}

	// TODO: Better way for this
	public Sprite GetItemSprite( ItemType item ) {
		return sprites[ (int)item ];
	}
}
