using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Ionic.Zlib;

// Modeled after TMX-format
// http://doc.mapeditor.org/reference/tmx-map-format/#imagelayer

namespace SA {
	public static class Tile {
		public static readonly System.UInt32 FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
		public static readonly System.UInt32 FLIPPED_VERTICALLY_FLAG   = 0x40000000;
		public static readonly System.UInt32 FLIPPED_DIAGONALLY_FLAG   = 0x20000000;
		public static readonly System.UInt32 FLIP_MASK = FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG;
				
		public static System.UInt32 UUID( System.UInt32 tile ) {
			return tile & ( ~FLIP_MASK );
		}

		public static bool FlippedHori( System.UInt32 tile ) {
			return ( tile & FLIPPED_HORIZONTALLY_FLAG) != 0;
		}

		public static void SetFlippedHori( ref System.UInt32 tile, bool flipped ) {
			tile = flipped ? ( tile | FLIPPED_HORIZONTALLY_FLAG ) : ( tile & ( ~FLIPPED_HORIZONTALLY_FLAG ) );
		}

		public static bool FlippedVert( System.UInt32 tile ) {
			return ( tile & FLIPPED_VERTICALLY_FLAG ) != 0;
		}

		public static void SetFlippedVert( ref System.UInt32 tile, bool flipped ) {
			tile = flipped ? ( tile | FLIPPED_VERTICALLY_FLAG ) : ( tile & ( ~FLIPPED_VERTICALLY_FLAG ) );
		}

		public static bool FlippedDiag( System.UInt32 tile ) {
			return ( tile & FLIPPED_DIAGONALLY_FLAG ) != 0;
		}

		public static void SetFlippedDiag( ref System.UInt32 tile, bool flipped ) {
			tile = flipped ? ( tile | FLIPPED_DIAGONALLY_FLAG ) : ( tile & ( ~FLIPPED_DIAGONALLY_FLAG ) );
		}

		// Omitted features:
		// terrain
		// probability

		// Local ID within its tileset
//		private uint id;
	}

	public class TileMapProperty {
		private string name;
		private string value;

		public string Name {
			get { return name; }
		}
		public string Value {
			get { return value; }
		}

		public TileMapProperty( string name, string value ) {
			this.name  = name;
			this.value = value;
		}
	}

	public class TileMapObject {
		// Omitted features:
		// rotation
		// gid
		// ellipsibus, polygon, polyline
		// image

		// Unique id of the object in a specific tile map
		private uint    id;
		private string  name;
		private string  objectType;
		private Rect    bounds;
		private bool    visible;

		private Dictionary<string, string> properties;

		public uint ID {
			get { return id; }
		}
		public string Name {
			get { return name; }
		}
		public string ObjectType {
			get { return objectType; }
		}
		public Rect Bounds {
			get { return bounds; }
		}
		public Vector2 Position {
			get { return bounds.position; }
		}
		public Size2 Size {
			get { return (Size2)bounds.size; }
		}
		public bool Visible {
			get { return visible; }
		}
		public Dictionary<string, string> Properties {
			get { return properties; }
		}

		public TileMapObject( uint id, string name, string objectType, Rect bounds, bool visible, Dictionary<string,string> properties ) {
			this.id = id;
			this.name = name;
			this.objectType = objectType;
			this.bounds = bounds;
			this.visible = visible;
			this.properties = properties;
		}
	}

	public class Tileset {
		// Omitted features:
		// tile offset
		// images
		// terraintypes
		// spacing
		// margin

		private string name;
		// Maximum size of tiles
		private Size2i tileSize;
		// Relative image path (from .tmx or .tsx file)
		private string imagePath;
		private Dictionary<string, string> properties;
		// TODO: Do this some other way?
		private System.UInt32[] uuids;

		public string Name {
			get { return name; }
		}
		public Size2i TileSize {
			get { return tileSize; }
		}
		public string ImagePath {
			get { return imagePath; }
		}
		public Dictionary<string, string> Properties {
			get { return properties; }
		}
		public System.UInt32[] UUIDs {
			get { return uuids; }
			set { uuids = value; }
		}

		public Tileset( string name, Size2i tileSize, string imagePath, Dictionary<string, string> properties ) {
			this.name = name;
			this.tileSize = tileSize;
			this.imagePath = imagePath;
			this.properties = properties;
		}
	}

	public class TilesetRef {
		// The global ID that maps to the first tile
		// in this tileset (in a tilemap)
		private System.UInt32 firstGID;
		private Tileset tileset;

		public uint FirstGID {
			get { return firstGID; }
		}
		public Tileset Value {
			get { return tileset; }
		}

		public TilesetRef( uint firstGID, Tileset tileset ) {
			this.firstGID = firstGID;
			this.tileset = tileset;
		}

		public static explicit operator Tileset( TilesetRef tilesetRef ) {
			return tilesetRef.tileset;
		}
	}

	public abstract class Layer {
		// Omitted features:
		// x, y, width, height (width/height always same as map since Tiled Qt)
		
		// Name of the layer
		protected string name;
		// Opacity of layer 
		protected float  opacity;
		// Is layer shown or hidden
		protected bool   visible;
		// Properties associated with this layer
		protected Dictionary<string, string> properties;
		
		public string Name {
			get { return name; }
		}
		public float Opacity {
			get { return opacity; }
		}
		public bool Visible {
			get { return visible; }
		}
		public Dictionary<string, string> Properties {
			get { return properties; }
		}
	}

	public class TileLayer : Layer {
		// Tile data
		private System.UInt32[] tiles;

		public System.UInt32[] Tiles {
			get { return tiles; }
		}

		public TileLayer( string name, float opacity, bool visible, Dictionary<string, string> properties, System.UInt32[] tiles ) {
			this.name = name;
			this.opacity = opacity;
			this.visible = visible;
			this.properties = properties;
			this.tiles = tiles;
		}
	}

	public class ObjectLayer : Layer {
		// Objects associated with this layer
		private TileMapObject[] objects;

		public TileMapObject[] Objects {
			get { return objects; }
		}

		public ObjectLayer( string name, float opacity, bool visible, Dictionary<string, string> properties, TileMapObject[] objects ) {
			this.name = name;
			this.opacity = opacity;
			this.visible = visible;
			this.properties = properties;
			this.objects = objects;
		}
	}

	public class TileMap {
		// Omitted features:
		// isometric & staggered orientation
		// image layers
		// render order

		// Size of the map in tiles
		private Size2i size;
		// Size of each tile in pixels
		private Size2i tileSize;
		// Color drawn behind tiles
		private Color  backgroundColor;

		private TilesetRef[]      tilesets;
		private TileLayer[]       tileLayers;
		private Dictionary<string, string> properties;
		private ObjectLayer[]     objectLayers;

		private int midgroundIndex;

		public Size2i Size {
			get { return size; }
		}
		public Size2i TileSize {
			get { return tileSize; }
		}
		public Color BackgroundColor {
			get { return backgroundColor; }
		}
		public TilesetRef[] Tilesets {
			get { return tilesets; }
		}
		public TileLayer[] TileLayers {
			get { return tileLayers; }
		}
		public Dictionary<string, string> Properties {
			get { return properties; }
		}
		public ObjectLayer[] ObjectLayers {
			get { return objectLayers; }
		}
		public int MidgroundLayerIndex {
			get { return midgroundIndex; }
		}
		public TileLayer MidgroundLayer {
			get { return tileLayers[ midgroundIndex ]; }
		}

		public TileMap( Size2i size, Size2i tileSize, Color bgColor,
		                TilesetRef[] tilesets, Dictionary<string, string> properties,
		                TileLayer[] tileLayers, ObjectLayer[] objectLayers,
		                int midgroundIndex ) {
			this.size            = size;
			this.tileSize        = tileSize;
			this.backgroundColor = bgColor;
			this.tilesets        = tilesets;
			this.properties      = properties;
			this.tileLayers      = tileLayers;
			this.objectLayers    = objectLayers;
			this.midgroundIndex = midgroundIndex;
		}
	}

	public static class TileMapTMXReader {
		public static TileMap ParseTMXFileAtPath( string filePath, TilesetLookup tilesetLookup ) {
			using( XmlReader reader = XmlReader.Create( new StreamReader( filePath ) ) ) {
				XElement map = XElement.Load( reader );
				return ParseTileMap( map, tilesetLookup, filePath );
			}
		}

		private static TileMap ParseTileMap( XElement map, TilesetLookup tilesetLookup, string filePath ) {
			// Assume orthogonal orientation and right-down renderorder
			var size = new Size2i( (int)map.Attribute( "width" ), (int)map.Attribute( "height" ) );
			var tileSize = new Size2i( (int)map.Attribute( "tilewidth" ), (int)map.Attribute( "tileheight" ) );
			
			var defaultColor = Color.white;
			var bgColor = ParseColorString( (string)map.Attribute( "backgroundcolor" ), defaultColor );

			var tilesets = new List<TilesetRef>();
			Dictionary<string, string> properties = null;
			var tileLayers = new List<TileLayer>();
			var objectLayers = new List<ObjectLayer>();
			int midgroundIndex = 0;

			foreach( var child in map.Elements() ) {
				if( child.Name == "tileset" ) {
					tilesets.Add( ParseTileset( child, tilesetLookup, filePath ) );
				} else if( child.Name == "properties" ) {
					properties = ParseProperties( child );
				} else if( child.Name == "layer" ) {
					var tileLayer = ParseTileLayer( child, size, tilesets, tilesetLookup );
					if( tileLayer.Name == "Midground" ) {
						midgroundIndex = tileLayers.Count;
					}
					tileLayers.Add( tileLayer );
				} else if( child.Name == "objectgroup" ) {
					objectLayers.Add( ParseObjectLayer( child ) );
				}
			}

			return new TileMap( size, tileSize, bgColor, tilesets.ToArray(), properties,
			                    tileLayers.ToArray(), objectLayers.ToArray(), midgroundIndex );
		}

		// TODO: Error check this mofo
		private static TilesetRef ParseTileset( XElement tileset, TilesetLookup tilesetLookup, string basePath ) {
			uint firstGID = (uint)tileset.Attribute( "firstgid" );
			// Relative path from TileMap
			string externalSource = (string)tileset.Attribute( "source" );
			string filePath = basePath;

			if( !string.IsNullOrEmpty( externalSource ) ) {
				filePath = Path.Combine( Path.GetDirectoryName( filePath ), externalSource );

				var existingTileset = tilesetLookup.GetTilesetByFilePath( filePath );
				if( existingTileset != null ) {
					return new TilesetRef( firstGID, existingTileset );
				}

				using( XmlReader reader = XmlReader.Create( new StreamReader( filePath ) ) ) {
					var externalTileset = XElement.Load( reader );
					return ParseTileset( externalTileset, firstGID, tilesetLookup, filePath );
				}
			}

			filePath = Path.Combine( filePath, tileset.Name + ".embedded" );

			return ParseTileset( tileset, firstGID, tilesetLookup, filePath );
		}

		private static TilesetRef ParseTileset( XElement tileset, uint firstGID, TilesetLookup tilesetLookup, string externalFilePath ) {
			string name = (string)tileset.Attribute( "name" );
			
			int w = (int) tileset.Attribute( "tilewidth" );
			int h = (int) tileset.Attribute( "tileheight" );
			var tileSize = new Size2i( w, h );
			
			string imagePath = (string)tileset.Element( "image" ).Attribute( "source" );
			
			var properties = ParseProperties( tileset.Element( "properties" ) );
			
			var t = new Tileset( name, tileSize, imagePath, properties );
			tilesetLookup.AddTileset( t, externalFilePath );
			return new TilesetRef( firstGID, t );
		}

		private static TileLayer ParseTileLayer( XElement tileLayer, Size2i mapSize, List<TilesetRef> tilesets, TilesetLookup tilesetLookup ) {
			string name;
			float  opacity;
			bool   visible;
			Dictionary<string, string> properties;
			ParseLayerAttributes( tileLayer, out name, out opacity, out visible, out properties );

			var dataElement = tileLayer.Element( "data" );
			// Check encoding etc
			System.UInt32[] tiles = DecompressTiles( dataElement.Value, mapSize );
			tiles = ResolveTiles( tiles, mapSize, tilesets, tilesetLookup );

			return new TileLayer( name, opacity, visible, properties, tiles );
		}

		private static ObjectLayer ParseObjectLayer( XElement objectLayer ) {
			string name;
			float  opacity;
			bool   visible;
			Dictionary<string, string> properties;
			ParseLayerAttributes( objectLayer, out name, out opacity, out visible, out properties );

			var objectsList = new List<TileMapObject>();
			foreach( var obj in objectLayer.Elements( "object" ) ) {
				objectsList.Add( ParseObject( obj ) );
			}

			return new ObjectLayer( name, opacity, visible, properties, objectsList.ToArray() );
		}

		private static TileMapObject ParseObject( XElement obj ) {
			uint id = (uint)obj.Attribute( "id" );
			string name = (string)obj.Attribute( "name" );
			string type = (string)obj.Attribute( "type" );

			float x = (float)obj.Attribute( "x" );
			float y = (float)obj.Attribute( "y" );
			float w = ( (float?)obj.Attribute( "width" ) ) ?? 0.0f;
			float h = ( (float?)obj.Attribute( "height" ) ) ?? 0.0f;
			Rect bounds = new Rect( x, y, w, h );

			var properties = ParseProperties( obj.Element( "properties" ) );

			bool visible = ( (bool?)obj.Attribute( "visible" ) ) ?? true;

			return new TileMapObject( id, name, type, bounds, visible, properties );
		}

		private static void ParseLayerAttributes( XElement layer, out string name, out float opacity,
		                                          out bool visible, out Dictionary<string, string> properties ) {
			name = (string)layer.Attribute( "name" );
			opacity = ( (float?)layer.Attribute( "opacity" ) ) ?? 1.0f;
			visible = ( (bool?)layer.Attribute( "visible" ) ) ?? true;
			properties = ParseProperties( layer.Element( "properties" ) );
		}

		private static Dictionary<string,string> ParseProperties( XElement properties ) {
			var ret = new Dictionary<string, string>();
			if( properties == null ) {
				return ret;
			}

			foreach( var property in properties.Elements() ) {
				string k = (string)property.Attribute( "name" );
				string v = (string)property.Attribute( "value" );
				if( !string.IsNullOrEmpty( k ) ) {
					ret[ k ] = v;
				}
			}

			return ret;
		}

		private static Color ParseColorString( string colorString, Color def ) {
			if( string.IsNullOrEmpty( colorString ) || colorString.Length != 7 ) {
				return def;
			}

			// Could be done much faster by converting to int and bitshifting
			// Format: #ffaaff
			System.UInt16 red, green, blue;
			try {
				red   = System.Convert.ToUInt16( colorString.Substring( 1, 2 ), 16 );
				green = System.Convert.ToUInt16( colorString.Substring( 3, 2 ), 16 );
				blue  = System.Convert.ToUInt16( colorString.Substring( 5, 2 ), 16 );
			} catch( System.Exception e ) {
				SA.Debug.LogError( "Invalid color string: " + colorString + " || " + e );
				return def;
			}

			return new Color( red / 255.0f, green / 255.0f, blue / 255.0f );
		}

		private static System.UInt32[] DecompressTiles( string base64EncodedTiles, Size2i expectedSize ) {
			// TODO: This nicer probably
			byte[] compressedData = System.Convert.FromBase64String( base64EncodedTiles );
			
			var compressedDataStream = new MemoryStream( compressedData );
			var decompressedDataStream = new MemoryStream();
			var zLibStream = new ZlibStream( compressedDataStream, CompressionMode.Decompress, true );
			Util.CopyStream( zLibStream, decompressedDataStream );
			zLibStream.Close();

			if( decompressedDataStream.Length != ( expectedSize.width * expectedSize.height ) * 4 ) {
				SA.Debug.LogError( "Decompressed tiles longer than expected!" );
				return null;
			}

			if( ( decompressedDataStream.Length % 4 ) != 0 ) {
				SA.Debug.LogError( "Decompressed tiles not 32bit" );
				return null;
			}

			var tiles = new System.UInt32[ expectedSize.width * expectedSize.height ];
			var binaryReader = new BinaryReader( decompressedDataStream );
			binaryReader.BaseStream.Position = 0;
			for( int i = 0; i < expectedSize.width * expectedSize.height; ++i ) {
				tiles[ i ] = binaryReader.ReadUInt32();
			}

			return tiles;
		}

		private static System.UInt32[] ResolveTiles( System.UInt32[] tiles, Size2i mapSize, List<TilesetRef> tilesets, TilesetLookup tilesetLookup ) {
			// TODO: If double buffers proves an issue flip y somehow else
			var resolvedTiles = new System.UInt32[ tiles.Length ];

			for( int tileIndex = 0; tileIndex < tiles.Length; ++tileIndex ) {
				System.UInt32 tileGID = tiles[ tileIndex ];
				System.UInt32 flipMask = tileGID & Tile.FLIP_MASK;
				// Clear flags; works with GIDs as well
				tileGID = Tile.UUID( tileGID );

				// Resolve tileset
				for( int i = tilesets.Count - 1; i >= 0; --i ) {
					var tilesetRef = tilesets[ i ];

					if( tilesetRef.FirstGID <= tileGID ) {
						// Since Y is flipped in tiled, flip it back here
						int x = tileIndex % mapSize.width;
						int y = tileIndex / mapSize.width;
						y = ( mapSize.height - 1 ) - y;
						int resolvedTileIndex = x + y * mapSize.width;

						System.UInt32 localGID = tileGID - tilesetRef.FirstGID;
						if( localGID >= tilesetRef.Value.UUIDs.Length ) {
//							Debug.LogWarning( "Tile at " + new SA.Vector2i( tileIndex % mapSize.width, tileIndex / mapSize.width ) + " has GID (" + localGID + ") > Assigned UUIDs (" + tilesetRef.Value.UUIDs.Length + ")" );
							resolvedTiles[ resolvedTileIndex ] = 0u;
							break;
						}
						System.UInt32 uuid = tilesetRef.Value.UUIDs[ localGID ];
						uuid |= flipMask;
						resolvedTiles[ resolvedTileIndex ] = uuid;
						break;
					}
				}
			}

			return resolvedTiles;
		}
	}
	
	public struct TileInfo {
		public readonly Sprite TileSprite;
		public readonly System.UInt32 UUID;

		public TileInfo( Sprite tileSprite, System.UInt32 UUID ) {
			this.TileSprite = tileSprite;
			this.UUID = UUID;
		}
	}

	public class TilesetLookup {
		private Dictionary<string, Tileset> tilesetsByFilePath;
		private Dictionary<string, Sprite[]> spritesByFilePath;
		private List<TileInfo> tiles;

		public List<TileInfo> Tiles {
			get { return tiles; }
		}

		public TilesetLookup() {
			tilesetsByFilePath = new Dictionary<string, Tileset>();
			spritesByFilePath = new Dictionary<string, Sprite[]>();
			tiles = new List<TileInfo>();
			// Reserve 0
			tiles.Add( new TileInfo( null, 0 ) );
		}

		// Absolute file path
		public Tileset GetTilesetByFilePath( string filePath ) {
			Tileset tileset;
			if( !tilesetsByFilePath.TryGetValue( filePath, out tileset ) ) {
				return null;
			}
			return tileset;
		}

		// Absolute file path
		public void AddTileset( Tileset tileset, string filePath ) {
			if( string.IsNullOrEmpty( tileset.Name ) || string.IsNullOrEmpty( filePath ) ) {
				SA.Debug.Log( "Must have name and file path" );
				return;
			}

			if( tilesetsByFilePath.ContainsKey( filePath ) ) {
				SA.Debug.LogError( "Duplicate tilesets: " + filePath );
			} else {
				tilesetsByFilePath[ filePath ] = tileset;
			}

			var tileSprites = LoadSpritesIfNecessary( tileset, filePath );
			tileset.UUIDs = new System.UInt32[ tileSprites.Length ];
			for( int i = 0; i < tileSprites.Length; ++i ) {
				System.UInt32 uuid = (System.UInt32) tiles.Count;
				tileset.UUIDs[ i ] = uuid;
				tiles.Add( new TileInfo( tileSprites[ i ], uuid ) );
			}
		}

		private Sprite[] LoadSpritesIfNecessary( Tileset tileset, string filePath ) {
			string imagePath = filePath;
			// Will contain full TileMap path + /[tileset.Name].embedded
			// so needs extra removal
			if( Path.GetExtension( imagePath ) == ".embedded" ) {
				imagePath = Path.GetDirectoryName( imagePath );
			}
			imagePath = Path.GetDirectoryName( imagePath );
			imagePath = Path.Combine( imagePath, tileset.ImagePath );

			if( spritesByFilePath.ContainsKey( imagePath ) ) {
				return spritesByFilePath[ imagePath ];
			}

			SA.Debug.Log( "Will load sprites at: " + imagePath );

			// TODO: Support sprites located other places than resources?
			if( imagePath.StartsWith( Util.ResourcesPath ) ) {
				// This needs a lot of massaging
				string relativeResourcesPath = imagePath;
				relativeResourcesPath = relativeResourcesPath.Remove( 0, Util.ResourcesPath.Length);
				relativeResourcesPath = relativeResourcesPath.Replace( "\\", "/" );
				if( relativeResourcesPath.StartsWith( "/" ) ) {
					relativeResourcesPath = relativeResourcesPath.Remove( 0, 1 );
				}
				relativeResourcesPath = relativeResourcesPath.Replace( Path.GetExtension( relativeResourcesPath ), "" );
				SA.Debug.Log( "Texture resource path: " + relativeResourcesPath );
				object loadedObj = Resources.Load( relativeResourcesPath );
				SA.Debug.Log( "Loaded object: " + loadedObj );
				Sprite[] sprites = Resources.LoadAll<Sprite>( relativeResourcesPath );
				SA.Debug.Log( "Loaded sprites: " + sprites );

				// Ensure that sprites are ordered by their position in
				// the atlas (messy, tired)
				System.Array.Sort( sprites, ( Sprite a, Sprite b ) => {
					var apos = a.textureRect.position;
					var bpos = b.textureRect.position;
					float w = a.textureRect.width;

					float ai = w - apos.x + apos.y * w;
					float bi = w - bpos.x + bpos.y * w;

					return (int)bi - (int)ai;
				} );

				spritesByFilePath[ imagePath ] = sprites;
				return sprites;
			}

			SA.Debug.Assert( false, "Unable to load sprites" );

			return null;
		}
	}

}