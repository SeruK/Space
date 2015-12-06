using UnityEngine;
using UE = UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SA {
	public class SpriteAtlas {
		public class Entry {
			public readonly string Name;
			public readonly Vector2i Position;
			public readonly Size2i Size;
			public readonly Vector2 Orig;

			public Entry( string name, Vector2i position, Size2i size, Vector2 orig ) {
				this.Name = name;
				this.Position = position;
				this.Size = size;
				this.Orig = orig;
			}
		}
		
		public readonly string ImageFile;
		public readonly Vector2i Size;
		//public readonly TextureFormat TextureFormat;
		public readonly Entry[] Entries;

		private SpriteAtlas( string imageFile, Vector2i size, List<Entry> entries ) {
			this.ImageFile = imageFile;
			this.Size = size;
			this.Entries = entries.ToArray();
		}

		public static SpriteAtlas FromFile( string absPath ) {
			string[] lines = File.ReadAllLines( absPath, Encoding.UTF8 );
			if( lines.Length < 5 ) {
				UE.Debug.LogError( "Invalid atlas header" );
				return null;
			}
			
			// Header format:
			//tiles.png
			//size: 128,128
			//format: RGBA8888
			//filter: Nearest,Nearest
			//repeat: none
			int i = 0;
			for( ; string.IsNullOrEmpty( lines[ i ] ); ++i ) {}

			string fileName = lines[ i++ ];
			Size2i atlasSize = Size2i.Parse( lines[ i++ ].Replace( "size: ", "" ) );
			//string formatStr = lines[ i++ ].Replace( "format: ", "" ); // Ignore format for now
			i += 3; // Skip filter and repeat, place i at first entry

			// Entry format is something like this
			//BACKGROUND
			//  rotate: false
			//  xy: 0, 100
			//  size: 20, 20
			//  orig: 20, 20
			//  offset: 0, 0
			//  index: -1
			List<Entry> entries = new List<Entry>();
			while( i < lines.Length ) {
				string entryName = lines[ i ];

				//UE.Debug.Log( "Found entry: " + entryName );

				Vector2i? position = null;
				Size2i? size = null;
				Vector2i? orig = null;
				while( ++i < lines.Length && lines[ i ].StartsWith( "  " ) ) {
					string[] kvp = lines[ i ].Split( ':' ).Select( x => x.Trim() ).ToArray();
					if( kvp.Length != 2 ) {
						UE.Debug.LogErrorFormat( "Invalid entry format (kvp wrong)" );
						return null;
					}
					string key = kvp[ 0 ];
					string val = kvp[ 1 ];
					//UE.Debug.LogFormat( "  \"{0}\": \"{1}\"", key, val );
					switch( key ) {
						case "xy":
							position = Vector2i.Parse( val ); break;
						case "size":
							size = Size2i.Parse( val ); break;
						case "orig":
							orig = Vector2i.Parse( val ); break;
						default: break;
					}
				}

				if( position == null || size == null ) {
					UE.Debug.LogErrorFormat( "Unable to find position or size of {0}", entryName );
					return null;
				}

				if( orig == null ) {
					UE.Debug.LogWarningFormat( "Unable to find orig of {0}, defaulting", entryName );
				}

				// This is flipped in Unity
				position = new Vector2i( position.Value.x, atlasSize.height - position.Value.y - size.Value.height );

				Vector2 origConv = orig == null ?
					new Vector2( 0.5f, 0.5f ) :
					new Vector2( ( (float)orig.Value.x ) / (float)size.Value.width, ( (float)orig.Value.y ) / (float)size.Value.height );

				var entry = new Entry( entryName, position.Value, size.Value, origConv );
				entries.Add( entry );
			}

			return new SpriteAtlas( fileName, atlasSize, entries );
		}
	}
}