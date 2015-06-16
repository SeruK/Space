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
		// The global ID that maps to the first tile
		// in this tileset
		private uint firstGID;
		// Maximum size of tiles
		private Size2i tileSize;
		// Relative image path (from .tmx or .tsx file)
		private string imagePath;
		private TileMapProperty[] properties;

		public string Name {
			get { return name; }
		}
		public uint FirstGID {
			get { return firstGID; }
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

		public Tileset( string name, uint firstGID, Size2i tileSize, string imagePath, TileMapProperty[] properties ) {
			this.name = name;
			this.firstGID = firstGID;
			this.tileSize = tileSize;
			this.imagePath = imagePath;
			this.properties = properties;
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

		private Tileset[]         tilesets;
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
		public Tileset[] Tilesets {
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
		                Tileset[] tilesets, TileMapProperty[] properties,
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
		public static TileMap ParseTMXFileAtPath( string filePath ) {
			using( XmlReader reader = XmlReader.Create( new StreamReader( filePath ) ) ) {
				XElement map = XElement.Load( reader );
				return ParseTileMap( map );
			}
		}

		private static TileMap ParseTileMap( XElement map ) {
			// Assume orthogonal orientation and right-down renderorder
			var size = new Size2i( (int)map.Attribute( "width" ), (int)map.Attribute( "height" ) );
			var tileSize = new Size2i( (int)map.Attribute( "tilewidth" ), (int)map.Attribute( "tileheight" ) );
			
			var bgColor = Color.white;
			var bgColorElement = map.Attribute( "backgroundcolor" );
			if( bgColorElement != null ) {
				bgColor = ParseColorString( bgColorElement.Value, bgColor );
			}

			var tilesets = new List<Tileset>();
			TileMapProperty[] properties = new TileMapProperty[ 0 ];
			var tileLayers = new List<TileLayer>();
			var objectLayers = new List<ObjectLayer>();

			foreach( var child in map.Elements() ) {
				if( child.Name == "tileset" ) {
					tilesets.Add( ParseTileset( child ) );
				} else if( child.Name == "properties" ) {
					properties = ParseProperties( child );
				} else if( child.Name == "layer" ) {
					tileLayers.Add( ParseTileLayer( child, size ) );
				}
			}

			return new TileMap( size, tileSize, bgColor, tilesets.ToArray(), properties,
			                    tileLayers.ToArray(), objectLayers.ToArray() );
		}

		private static Tileset ParseTileset( XElement tileset) {
			string externalSource = tileset.Attribute( "source" ).Value;

			if( !string.IsNullOrEmpty( externalSource ) ) {
				Debug.Log( "External tileset" );
				return null;
			}

			string name = tileset.Attribute( "name" ).Value;
			uint firstGID = (uint)tileset.Attribute( "firstGID" );

			int w = (int) tileset.Attribute( "tilewidth" );
			int h = (int) tileset.Attribute( "tileheight" );
			var tileSize = new Size2i( w, h );

			string imagePath = tileset.Element( "image ").Attribute( "source" ).Value;

			var properties = ParseProperties( tileset.Element( "properties" ) );

			return new Tileset( name, firstGID, tileSize, imagePath, properties );
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

		private static ObjectLayer ParseObjectLayer( XElement objectLayer, Size2i mapSize ) {
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
//			private uint    id;
//			private string  name;
//			private string  type;
//			private Rect    bounds;
//			private bool    visible;
//			
//			private TileMapProperty[] properties;
			uint id = (uint)obj.Attribute( "id" );
			string name = (string)obj.Attribute( "name" );
			string type = (string)obj.Attribute( "type" );

			float x = (float)obj.Attribute( "x" );
			float y = (float)obj.Attribute( "y" );
			float w = (float)obj.Attribute( "w" );
			float h = (float)obj.Attribute( "h" );
			Rect bounds = new Rect( x, y, w, h );

			TileMapProperty[] properties = ParseProperties( obj.Element( "properties" ) );

			bool visible = ( (bool?)obj.Attribute( "visible" ) ) ?? true;

			return new TileMapObject( id, name, type, bounds, visible, properties );
		}

		private static void ParseLayerAttributes( XElement layer, out string name, out float opacity,
		                                          out bool visible, out TileMapProperty[] properties ) {
			name = layer.Attribute( "name" ).Value;
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
			string k = property.Attribute( "name" ).Value;
			string v = property.Attribute( "value" ).Value;
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

			var tiles = new System.UInt32[ expectedSize.width * expectedSize.height ];
			var binaryReader = new BinaryReader( decompressedDataStream );
			int i = 0;
			while( decompressedDataStream.Position < decompressedDataStream.Length ) {
				tiles[ i ] = binaryReader.ReadUInt32();
				++i;
			}

			return tiles;
		}
	}
}