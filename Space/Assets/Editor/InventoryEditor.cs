using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor( typeof(Inventory) )]
public class InventoryEditor : Editor {
	override public void OnInspectorGUI() {
		DrawDefaultInspector();

		var inventory = target as Inventory;
		if( inventory == null ) {
			return;
		}

		int numItems = System.Enum.GetValues( typeof(Item.ItemType) ).Length;
		if( inventory.sprites.Length != numItems ) {
			var old = inventory.sprites;
			inventory.sprites = new Sprite[ numItems ];
			for( int i = 0; i < Mathf.Min( old.Length, inventory.sprites.Length); ++i ) {
				inventory.sprites[ i ] = old[ i ];
			}
		}

		string[] names = System.Enum.GetNames( typeof(Item.ItemType) );

		for( int i = 0; i < inventory.sprites.Length; ++i ) {
			bool allowSceneObjects = false;
			inventory.sprites[ i ] = EditorGUILayout.ObjectField(names[i],
			                                                     inventory.sprites[ i ] as UnityEngine.Object,
			                                                     typeof( Sprite ),
			                                                     allowSceneObjects ) as Sprite;
			if( GUI.changed ) {
				EditorUtility.SetDirty( target );
			}
		}
	}
}