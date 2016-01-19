using UnityEngine;
using UE = UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SA;

namespace SA {
	public class SpritePackingPostProcessor : AssetPostprocessor {
		private static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths ) {
			var atlases = from asset in importedAssets where asset.IndexOf( ".atlas" ) != -1 select asset;
			//Debug.LogFormat( "ASSET POSTPROCESS FOUND {0} ATLASES", atlases.Count() );
			foreach( var atlas in atlases ) {
				ProcessSpriteSheet( atlas );
			}
		}

		private static void ProcessSpriteSheet( string atlasPath ) {
			UE.Debug.Log( "Processing " + atlasPath );
			string absPath = Path.GetDirectoryName( Application.dataPath ) + "/" + atlasPath;

			SpriteAtlas atlas = SpriteAtlas.FromFile( absPath );

			string textureAssetPath = Path.GetDirectoryName( atlasPath ) + "/" + atlas.ImageFile;

			var importer = AssetImporter.GetAtPath( textureAssetPath ) as TextureImporter;

			if( importer == null ) {
				UE.Debug.LogError( "Unable to get asset importer for \"{0}\"", importer );
				return;
			}

			// TODO: This a better way
			bool isTileset = atlasPath.Contains( "Tilesets" );

			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			var spritesheet = new SpriteMetaData[ atlas.Entries.Length ];

			for( int i = 0; i < atlas.Entries.Length; ++i ) {
				SpriteAtlas.Entry entry = atlas.Entries[ i ];
				spritesheet[ i ].name = entry.Name;
				spritesheet[ i ].rect = new Rect( (Vector2)entry.Position, (Vector2)entry.Size );
				spritesheet[ i ].pivot = entry.Orig;
			}

			importer.spritesheet = spritesheet;
			importer.filterMode = FilterMode.Point;
			if( isTileset ) {
				importer.spritePixelsPerUnit = Constants.PIXELS_PER_UNIT ;
			}
		}
	}
}
