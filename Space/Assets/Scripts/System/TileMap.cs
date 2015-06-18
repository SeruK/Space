using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Ionic.Zlib;

// Modeled after TMX-format
// http://doc.mapeditor.org/reference/tmx-map-format/#imagelayer

namespace SA {
	public class TileMapTile {
		// Omitted features:
		// terrain
		// probability

		// Local ID within its tileset
		private uint id;
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

		private TileMapProperty[] properties;

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
		public TileMapProperty[] Properties {
			get { return properties; }
		}

		public TileMapObject( uint id, string name, string objectType, Rect bounds, bool visible, TileMapProperty[] properties ) {
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
		private TileMapProperty[] properties;

		public string Name {
			get { return name; }
		}
		public Size2i TileSize {
			get { return tileSize; }
		}
		public string ImagePath {
			get { return imagePath; }
		}
		public TileMapProperty[] Properties {
			get { return properties; }
		}

		public Tileset( string name, Size2i tileSize, string imagePath, TileMapProperty[] properties ) {
			this.name = name;
			this.tileSize = tileSize;
			this.imagePath = imagePath;
			this.properties = properties;
		}
	}

	public class TilesetRef {
		// The global ID that maps to the first tile
		// in this tileset (in a tilemap)
		private uint firstGID;
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
		protected TileMapProperty[] properties;
		
		public string Name {
			get { return name; }
		}
		public float Opacity {
			get { return opacity; }
		}
		public bool Visible {
			get { return visible; }
		}
		public TileMapProperty[] Properties {
			get { return properties; }
		}
	}

	public class TileLayer : Layer {
		// Tile data
		private System.UInt32[] tiles;

		public System.UInt32[] Tiles {
			get { return tiles; }
		}

		public TileLayer( string name, float opacity, bool visible, TileMapProperty[] properties, System.UInt32[] tiles ) {
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

		public ObjectLayer( string name, float opacity, bool visible, TileMapProperty[] properties, TileMapObject[] objects ) {
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
		private TileMapProperty[] properties;
		private ObjectLayer[]     objectLayers;

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
		public TileMapProperty[] Properties {
			get { return properties; }
		}
		public ObjectLayer[] ObjectLayers {
			get { return objectLayers; }
		}

		public TileMap( Size2i size, Size2i tileSize, Color bgColor,
		                TilesetRef[] tilesets, TileMapProperty[] properties,
		                TileLayer[] tileLayers, ObjectLayer[] objectLayers ) {
			this.size            = size;
			this.tileSize        = tileSize;
			this.backgroundColor = bgColor;
			this.tilesets        = tilesets;
			this.properties      = properties;
			this.tileLayers      = tileLayers;
			this.objectLayers    = objectLayers;
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
			TileMapProperty[] properties = new TileMapProperty[ 0 ];
			var tileLayers = new List<TileLayer>();
			var objectLayers = new List<ObjectLayer>();

			foreach( var child in map.Elements() ) {
				if( child.Name == "tileset" ) {
					tilesets.Add( ParseTileset( child, tilesetLookup, filePath ) );
				} else if( child.Name == "properties" ) {
					properties = ParseProperties( child );
				} else if( child.Name == "layer" ) {
					tileLayers.Add( ParseTileLayer( child, size ) );
				} else if( child.Name == "objectgroup" ) {
					objectLayers.Add( ParseObjectLayer( child ) );
				}
			}

			return new TileMap( size, tileSize, bgColor, tilesets.ToArray(), properties,
			                    tileLayers.ToArray(), objectLayers.ToArray() );
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

		private static TileLayer ParseTileLayer( XElement tileLayer, Size2i mapSize ) {
			string name;
			float  opacity;
			bool   visible;
			TileMapProperty[] properties;
			ParseLayerAttributes( tileLayer, out name, out opacity, out visible, out properties );

			var dataElement = tileLayer.Element( "data" );
			// Check encoding etc
			System.UInt32[] tiles = DecompressTiles( dataElement.Value, mapSize );

			return new TileLayer( name, opacity, visible, properties, tiles );
		}

		private static ObjectLayer ParseObjectLayer( XElement objectLayer ) {
			string name;
			float  opacity;
			bool   visible;
			TileMapProperty[] properties;
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

			TileMapProperty[] properties = ParseProperties( obj.Element( "properties" ) );

			bool visible = ( (bool?)obj.Attribute( "visible" ) ) ?? true;

			return new TileMapObject( id, name, type, bounds, visible, properties );
		}

		private static void ParseLayerAttributes( XElement layer, out string name, out float opacity,
		                                          out bool visible, out TileMapProperty[] properties ) {
			name = (string)layer.Attribute( "name" );
			opacity = ( (float?)layer.Attribute( "opacity" ) ) ?? 1.0f;
			visible = ( (bool?)layer.Attribute( "visible" ) ) ?? true;
			properties = ParseProperties( layer.Element( "properties" ) );
		}

		private static TileMapProperty[] ParseProperties( XElement properties ) {
			if( properties == null ) {
				return new TileMapProperty[ 0 ];
			}

			var propertiesList = new List<TileMapProperty>();
			foreach( var property in properties.Elements() ) {
				propertiesList.Add( ParseProperty( property ) );
			}
			return propertiesList.ToArray();
		}

		private static TileMapProperty ParseProperty( XElement property ) {
			string k = (string)property.Attribute( "name" );
			string v = (string)property.Attribute( "value" );
			return new TileMapProperty( k, v );
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
				DebugUtil.LogError( "Invalid color string: " + colorString + " || " + e );
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
				DebugUtil.LogError( "Decompressed tiles longer than expected!" );
				return null;
			}

			if( ( decompressedDataStream.Length % 4 ) != 0 ) {
				DebugUtil.LogError( "Decompressed tiles not 32bit" );
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
	}

	public class TilesetLookup {
		private Dictionary<string, Tileset> tilesetsByName;
		private Dictionary<string, Tileset> tilesetsByFilePath;

		public TilesetLookup() {
			tilesetsByName = new Dictionary<string, Tileset>();
			tilesetsByFilePath = new Dictionary<string, Tileset>();
		}

		public Tileset GetTileset( string name ) {
			Tileset tileset;
			if( !tilesetsByName.TryGetValue( name, out tileset ) ) {
				return null;
			}
			return tilesetsByName[ name ];
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
				DebugUtil.Log( "Must have name and file path" );
				return;
			}

			if( tilesetsByName.ContainsKey( tileset.Name ) ) {
				DebugUtil.LogError( "Duplicate tilesets: " + tileset.Name );
			} else {
				tilesetsByName[ tileset.Name ] = tileset;
			}

			if( tilesetsByFilePath.ContainsKey( filePath ) ) {
				DebugUtil.LogError( "Duplicate tilesets: " + filePath );
			} else {
				tilesetsByFilePath[ filePath ] = tileset;
			}

			LoadSpriteIfNecessary( tileset, filePath );
		}

		private void LoadSpriteIfNecessary( Tileset tileset, string filePath ) {
			string imagePath = filePath;
			// Will contain full TileMap path + /[tileset.Name].embedded
			// so needs extra removal
			if( Path.GetExtension( imagePath ) == ".embedded" ) {
				imagePath = Path.GetDirectoryName( imagePath );
			}
			imagePath = Path.GetDirectoryName( imagePath );
			imagePath = Path.Combine( imagePath, tileset.ImagePath );
			DebugUtil.Log( "Will load image at: " + imagePath );
		}
	}
}