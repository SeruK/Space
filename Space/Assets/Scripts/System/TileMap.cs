using UnityEngine;
using System.Collections.Generic;
using System.Xml;
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

		public static void ReadTMXNodes( XmlReader reader, ref TileMapProperty[] properties ) {
			DebugUtil.Log( "<properties>" );
			var list = new List<TileMapProperty>();

			if( reader.ReadToDescendant( "property" ) ) {
				do {
					var prop = new TileMapProperty();
					prop.ReadTMXNode( reader );
					list.Add( prop );
				} while( reader.ReadToNextSibling( "property" ) );
			}
			properties = list.ToArray();
			DebugUtil.Log( "</properties>" );
		}

		public void ReadTMXNode( XmlReader reader ) {
			DebugUtil.Log( "<property>" );
			name  = reader.GetAttribute( "name" );
			value = reader.GetAttribute( "value" );
			DebugUtil.Log( "name: " + name );
			DebugUtil.Log( "value: " + value );	
			DebugUtil.Log( "</property>" );
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
		private string  type;
		private Rect    bounds;
		private bool    visible;

		private TileMapProperty[] properties;

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

		public void ReadTMXNode( XmlReader reader ) {
			DebugUtil.Log( "<object>" );
			uint.TryParse( reader.GetAttribute( "id" ), out id );
			name = reader.GetAttribute( "name" );
			type = reader.GetAttribute( "type" );
			float x, y, w, h;
			float.TryParse( reader.GetAttribute( "x" ), out x );
			float.TryParse( reader.GetAttribute( "y" ), out y );
			float.TryParse( reader.GetAttribute( "w" ), out w );
			float.TryParse( reader.GetAttribute( "h" ), out h );
			bounds.Set( x, y, w, h );
			bool.TryParse( reader.GetAttribute( "visible" ), out visible );

			DebugUtil.Log( "id: " + id );
			DebugUtil.Log( "name: " + name );
			DebugUtil.Log( "type: " + type );
			DebugUtil.Log( "bounds: " + bounds );
			DebugUtil.Log( "visible: " + visible );

			if( reader.ReadToDescendant( "properties" ) ) {
				TileMapProperty.ReadTMXNodes( reader, ref properties );
			}
			DebugUtil.Log( "</object>" );
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

		public void ReadTMXNode( XmlReader reader, string rootPath ) {
			// GID always specific here
			uint.TryParse( reader.GetAttribute( "firstgid" ), out firstGID );

			string sourceRelPath = reader.GetAttribute( "source" );
			if( sourceRelPath != null ) {
				string fullPath = Path.Combine( rootPath, sourceRelPath );
				using( var newReader = XmlReader.Create( new StreamReader( fullPath ) ) ) {
					if( !newReader.ReadToFollowing( "tileset" ) ) {
						Debug.LogWarning( "Invalid external tileset (" + sourceRelPath + ")" );
						return;
					}
					ReadTMXNodeInternal( newReader );
				}
			} else {
				ReadTMXNodeInternal( reader );
			}
		}

		private void ReadTMXNodeInternal( XmlReader reader ) {
			DebugUtil.Log( "<tileset>" );
			name = reader.GetAttribute( "name" );
			int.TryParse( reader.GetAttribute( "tilewidth" ),  out tileSize.width );
			int.TryParse( reader.GetAttribute( "tileheight" ), out tileSize.height );

			DebugUtil.Log( "name: " + name );
			DebugUtil.Log( "tileSize: " + tileSize );

			if( reader.ReadToDescendant( "image" ) ) {
				DebugUtil.Log( "<image>" );
				imagePath = reader.GetAttribute( "source" );
				DebugUtil.Log( "imagePath: " + imagePath );
				DebugUtil.Log( "</image>" );
			}
			DebugUtil.Log( "</tileset>" );
		}
	}

	public abstract class Layer {
		// Omitted features:
		// x, y, width, height (width/height always same as map since Tiled Qt)
		
		// Name of the layer
		private string name;
		// Opacity of layer 
		private float  opacity;
		// Is layer shown or hidden
		private bool   visible;
		// Properties associated with this layer
		private TileMapProperty[] properties;
		
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

		public virtual void ReadTMXNode( XmlReader reader ) {
			name = reader.GetAttribute( "name" );
			float.TryParse( reader.GetAttribute( "opacity" ), out opacity );
			bool.TryParse( reader.GetAttribute( "visible" ), out visible );

			DebugUtil.Log( "name: " + name );
			DebugUtil.Log( "opacity: " + opacity );
			DebugUtil.Log( "visible: " + visible );

			if( reader.ReadToDescendant( "properties" ) ) {
				TileMapProperty.ReadTMXNodes( reader, ref properties );
			}
		}
	}

	public class TileLayer : Layer {
		// Tile data
		private System.UInt32[] tiles;

		public System.UInt32[] Tiles {
			get { return tiles; }
		}

		public void ReadTMXNode( XmlReader reader, Tileset[] tilesets ) {
			DebugUtil.Log( "<layer>" );
			base.ReadTMXNode( reader );
			if( reader.ReadToDescendant( "data" ) ) {
				DebugUtil.Log( "<data>" );

				string encoding = reader.GetAttribute( "encoding" );
				string compression = reader.GetAttribute( "compression" );
				if( encoding != "base64" || compression != "zlib" ) {
					DebugUtil.LogWarn( "Not reading data (" + encoding + " | " + compression + ")" );
					DebugUtil.Log( "</data>" );
					return;
				}

				// TODO: This nicer probably
				string base64EncodedTiles = reader.ReadContentAsString();
				byte[] compressedData = System.Convert.FromBase64String( base64EncodedTiles );

				var compressedDataStream = new MemoryStream( compressedData );
				var decompressedDataStream = new MemoryStream();
				var zLibStream = new ZlibStream( compressedDataStream, CompressionMode.Decompress, true );
				Util.CopyStream( zLibStream, decompressedDataStream );
				zLibStream.Close();

				byte[] tilesAsBytes = decompressedDataStream.ToArray();

				DebugUtil.Log( "</data>" );
			}
			DebugUtil.Log( "</layer>" );
		}

		private void ReadTiles( System.UInt32[] data ) {

		}
	}

	public class ObjectLayer : Layer {
		// Objects associated with this layer
		private TileMapObject[] objects;

		public override void ReadTMXNode( XmlReader reader ) {
			DebugUtil.Log( "<objectgroup>" );
			base.ReadTMXNode( reader );

			var objectsList = new List<TileMapObject>();

			while( reader.ReadToDescendant( "object" ) ) {
				var tileMapObject = new TileMapObject();
				tileMapObject.ReadTMXNode( reader );
				objectsList.Add( tileMapObject );
			}
			objects = objectsList.ToArray();
			DebugUtil.Log( "</objectgroup>" );
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

		private TileMap() {
		}

		public void ReadTMXNode( XmlReader reader, string rootPath ) {
			DebugUtil.Log( "<tilemap>" );

			// Assume orthogonal & right-down render order
			int.TryParse( reader.GetAttribute( "width" ), out size.width );
			int.TryParse( reader.GetAttribute( "height" ), out size.height );
			int.TryParse( reader.GetAttribute( "tilewidth" ), out tileSize.width );
			int.TryParse( reader.GetAttribute( "tileheight" ), out tileSize.height );

			DebugUtil.Log( "size: " + size );
			DebugUtil.Log( "tileSize: " + tileSize );

			string bgColorStr = reader.GetAttribute( "backgroundcolor" );
			if( bgColorStr != null ) {
				// Could be done much faster by converting to int and bitshifting
				// Format: #ffaaff
				var red   = System.Convert.ToUInt16( bgColorStr.Substring( 1, 2 ), 16 );
				var green = System.Convert.ToUInt16( bgColorStr.Substring( 3, 2 ), 16 );
				var blue  = System.Convert.ToUInt16( bgColorStr.Substring( 5, 2 ), 16 );

				backgroundColor.r = (float)red / 255.0f;
				backgroundColor.g = (float)green / 255.0f;
				backgroundColor.b = (float)blue / 255.0f;
			}
			DebugUtil.Log( "backgroundcolor: " + backgroundColor);

			var tilesetsList     = new List<Tileset>();
			var tileLayersList   = new List<TileLayer>();
			var objectLayersList = new List<ObjectLayer>();

			while( reader.Read() ) {
				if( reader.NodeType == XmlNodeType.Element ) {
					if( reader.Name == "tileset" ) {
						var tileset = new Tileset();
						tileset.ReadTMXNode( reader, rootPath );
						tilesetsList.Add( tileset );
						continue;
					} 

					if( tilesetsList != null ) {
						tilesets = tilesetsList.ToArray();
						tilesetsList = null;
						DebugUtil.Log( "- FINISHED READING TILESETS - " );
					}

					if( reader.Name == "properties" ) {
						TileMapProperty.ReadTMXNodes( reader, ref properties );
					} else if( reader.Name == "layer" ) {
						var tileLayer = new TileLayer();
						tileLayer.ReadTMXNode( reader, tilesets );
						tileLayersList.Add( tileLayer );
					} else if( reader.Name == "objectgroup" ) {
						var objectLayer = new ObjectLayer();
						objectLayer.ReadTMXNode( reader );
						objectLayersList.Add( objectLayer );
					}
				}
			}

			tileLayers   = tileLayersList.ToArray();
			objectLayers = objectLayersList.ToArray();

			DebugUtil.Log( "</tilemap>" );
		}

		public static TileMap ParseTMXFileAtPath( string filePath ) {
			TileMap tileMap = new TileMap();

			using( XmlReader reader = XmlReader.Create( new StreamReader( filePath ) ) ) {
				reader.MoveToContent();
				tileMap.ReadTMXNode( reader, filePath );
			}

			return tileMap;
		}
	}
}