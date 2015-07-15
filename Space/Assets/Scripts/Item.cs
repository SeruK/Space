﻿using UnityEngine;
using System.Collections;

public static class Item {
	public enum ItemType {
		None,
		Gun,
		HealthPack,
		Binoculars,
		Compass,
		AlienMeat,
		DataDisc,
		TILE = 256
	}

	public static ItemType ItemTypeFromString( string str ) {
		ItemType itemType = ItemType.None;
		try {
			itemType = (ItemType)System.Enum.Parse( typeof(ItemType), str );
		} catch( System.Exception e ) {
			DebugUtil.LogWarn( "Invalid item type: " + str + "\n" + e );
		}
		return itemType;
	}

	public static string LocalizedNameId( ItemType itemType ) {
		if( IsTile( itemType ) ) {
			return "item_tile_" + TileUUIDFromItem( itemType ).ToString().PadLeft( 4, '0' );
		}

		return "item_" + System.Enum.GetName( typeof( ItemType ), itemType );
	}

	public static string FallbackName( ItemType itemType ) {
		if( IsTile( itemType ) ) {
			return "Tile (" + TileUUIDFromItem( itemType ).ToString() + ")";
		}

		return System.Enum.GetName( typeof( ItemType ), itemType );
	}

	public static bool IsTile( ItemType itemType ) {
		return (int)itemType >= (int)ItemType.TILE;
	}

	public static System.UInt32 TileUUIDFromItem( ItemType itemType, System.UInt32 defaultUUID ) {
		if( !IsTile( itemType ) ) {
			return defaultUUID;
		}
		return (uint)itemType - (uint)ItemType.TILE;
	}

	public static System.UInt32 TileUUIDFromItem( ItemType itemType ) {
		DebugUtil.Assert( IsTile( itemType ) );
		return (uint)itemType - (uint)ItemType.TILE;
	}

	public static ItemType ItemFromTileUUID( System.UInt32 uuid ) {
		return (ItemType)( uuid + (uint)ItemType.TILE );
	}
}
