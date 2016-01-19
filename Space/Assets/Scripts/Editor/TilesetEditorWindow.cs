using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SA;
using System.Linq;
using System.IO;

public class TilesetEditorWindow : EditorWindow {
	[MenuItem( "Window/Custom/Tileset" )]
	private static void MenuOpen() {
		EditorWindow.GetWindow<TilesetEditorWindow>( "Tileset" );
	}

	private TilesetLookup tilesetLookup;
	private Tileset tileset;
	private SpriteAtlas atlas;
	private bool gui_expanded;
	private Vector2 gui_scrollPos;

	private void OnEnable() {
		Load();
	}

	private void Load() {
		tilesetLookup = new TilesetLookup();
		var path = Application.dataPath + "/Resources/Tilesets/tiles/tiles";
		var tsxPath = path + ".tsx";
		tileset = TileMapTMXReader.ParseTSXFileAtPath( tsxPath );
		tilesetLookup.AddTileset( tileset, tsxPath );
		atlas = SpriteAtlas.FromFile( path + ".atlas" );
		
	}

	private void OnGUI() {
		if( atlas != null ) {
			gui_expanded = EditorGUILayout.Foldout( gui_expanded, tileset.Name );
			if( gui_expanded ) {
				gui_scrollPos = GUILayout.BeginScrollView( gui_scrollPos );
				DrawAtlas( atlas );
				GUILayout.EndScrollView();
			}
		}
	}

	private void DrawAtlas( SpriteAtlas atlas ) {
		int uuid = 0;
		foreach( SpriteAtlas.Entry entry in atlas.Entries ) {
			GUILayout.Label( uuid + " : " + entry.Name );
			++uuid;
		}
	}
}
