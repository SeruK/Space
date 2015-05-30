using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;

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
	}

	public class Tileset {
		// Omitted features:
		// tile offset
		// images
		// terraintypes
		// spacing
		// margin

		// The global ID that maps to the first tile
		// in this tileset
		private uint firstGID;
		// Maximum size of tiles
		private Size2i tileSize;

		public uint FirstGID {
			get { return firstGID; }
		}

		public Size2i TileSize {
			get { return tileSize; }
		}
	}

	public class Layer {
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

		}
	}

	public class TileLayer : Layer {
		// Tile data
		private System.UInt32[] tiles;

		public System.UInt32[] Tiles {
			get { return tiles; }
		}
	}

	public class ObjectLayer : Layer {
		// Objects associated with this layer
		private TileMapObject[]   objects;
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

		public void ReadTMXNode( XmlReader reader ) {
			// Assume orthogonal & right-down render order
			int.TryParse( reader.GetAttribute( "width" ), out size.width );
			int.TryParse( reader.GetAttribute( "height" ), out size.height );
			int.TryParse( reader.GetAttribute( "tilewidth" ), out tileSize.width );
			int.TryParse( reader.GetAttribute( "tileheight" ), out tileSize.height );

			DebugUtil.Log( "size: " + size );
			DebugUtil.Log( "tileSize: " + tileSize );

			string bgColorStr = reader.GetAttribute( "backgroundcolor" );
			DebugUtil.Log( "backgroundcolor: " + bgColorStr);
			if( bgColorStr != null ) {
				// Format: #ffaaff
				var red   = System.Convert.ToUInt16( bgColorStr.Substring( 1, 2 ), 16 );
				var green = System.Convert.ToUInt16( bgColorStr.Substring( 3, 2 ), 16 );
				var blue  = System.Convert.ToUInt16( bgColorStr.Substring( 5, 2 ), 16 );

				DebugUtil.Log( string.Format("BGColor: {0}, {1}, {2}", red, green, blue) );
				backgroundColor.r = (float)red / 255.0f;
				backgroundColor.g = (float)green / 255.0f;
				backgroundColor.b = (float)blue / 255.0f;
			}


		}

		public static TileMap ParseTMXFileAtPath( string filePath ) {
			TileMap tileMap = new TileMap();

			var tileLayers = new List<TileLayer>();
			var properties = new List<TileMapProperty>();
			var objectLayers = new List<ObjectLayer>();

			using( XmlReader reader = XmlReader.Create( new StreamReader( filePath ) ) ) {
				if( !reader.ReadToFollowing( "map" ) ) {
					DebugUtil.Log( "Unable to read to map element" );
					return null;
				}
				tileMap.ReadTMXNode( reader );
			}

			tileMap.tileLayers = tileLayers.ToArray();
			tileMap.properties = properties.ToArray();
			tileMap.objectLayers = objectLayers.ToArray();

			return tileMap;
		}
	}
}