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
		private TileMapObject[] objects;
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
	}

	public static class TileMapTMXReader {
		public static TileMap ParseTMXFileAtPath( string filePath ) {
			return null;
		}

		private static Color ParseColorString( string colorString, Color def ) {
			if( string.IsNullOrEmpty( colorString ) || colorString.Length != 7 ) {
				return def;
			}

			// Could be done much faster by converting to int and bitshifting
			// Format: #ffaaff
			var red   = System.Convert.ToUInt16( colorString.Substring( 1, 2 ), 16 );
			var green = System.Convert.ToUInt16( colorString.Substring( 3, 2 ), 16 );
			var blue  = System.Convert.ToUInt16( colorString.Substring( 5, 2 ), 16 );
			
			return new Color( red / 255.0f, green / 255.0f, blue / 255.0f );
		}

		private static byte[] DecompressTiles( string base64EncodedTiles ) {
			// TODO: This nicer probably
			byte[] compressedData = System.Convert.FromBase64String( base64EncodedTiles );
			
			var compressedDataStream = new MemoryStream( compressedData );
			var decompressedDataStream = new MemoryStream();
			var zLibStream = new ZlibStream( compressedDataStream, CompressionMode.Decompress, true );
			Util.CopyStream( zLibStream, decompressedDataStream );
			zLibStream.Close();
			
			byte[] tilesAsBytes = decompressedDataStream.ToArray();
			return tilesAsBytes;
		}
	}
}