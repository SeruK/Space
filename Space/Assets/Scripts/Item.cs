using UnityEngine;
using System.Collections;

public static class Item {
	public enum ItemType {
		None,
		Gun,
		HealthPack,
		Binoculars,
		Compass,
		AlienMeat,
		DataDisc
	}

	public static string LocalizedNameId( ItemType itemType ) {
		return "item_" + System.Enum.GetName( typeof( ItemType ), itemType );
	}

	public static string FallbackName( ItemType itemType ) {
		return System.Enum.GetName( typeof( ItemType ), itemType );
	}
}
