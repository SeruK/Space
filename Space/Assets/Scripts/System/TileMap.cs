using UnityEngine;

// Modeled after TMX-format
// http://doc.mapeditor.org/reference/tmx-map-format/#imagelayer

namespace SA {
	public class TileMapProperty {

	}

	public class TileMapObject {
		private uint    id;
		private string  name;
		private Rect    bounds;
		//private float   rotation;
		//private uint    gid;
		private bool    visible;

		public Rect Bounds {
			get { return bounds; }
		}

		public Vector2 Position {
			get { return bounds.position; }
		}

		public Size2 Size {
			get { return (Size2)bounds.size; }
		}
	}

	public class TileMapObjectGroup {
		private TileMapProperty[] properties;
		private TileMapObject[]   objects;
	}

	public class Tileset {
		// The global ID that maps to the first tile
		// in this tileset
		private uint firstGID;

		// External tilesets
		//private string source

		// Maximum size of tiles
		private Size2i tileSize;
		// Spacing in pixels between tiles
		private int    spacing;
		// Offset in pixels
		private Vector2i tileOffset;

		public uint FirstGID {
			get { return firstGID; }
		}

		public Size2i TileSize {
			get { return tileSize; }
		}

		public int Spacing {
			get { return spacing; }
		}
	}

	public class TileLayer {
		// Name of the layer
		private string name;
		// Opacity of layer 
		private float  opacity;
		// Is layer shown or hidden
		private bool   visible;
		// Tile data
		private System.UInt32[] tiles;

		public string Name {
			get { return name; }
		}
		public float Opacity {
			get { return opacity; }
		}
		public bool Visible {
			get { return visible; }
		}
	}

	public class TileMap {
		// Id
		private int    id;
		// Size of the map in tiles
		private Size2i size;
		// Size of each tile in pixels
		private Size2i tileSize;
		// Color drawn behind tiles
		private Color  backgroundColor;

		private TileLayer[]          layers;
		private TileMapProperty[]    properties;
		private TileMapObjectGroup[] objectGroups;

		public Size2i Size {
			get { return size; }
		}
		public Size2i TileSize {
			get { return tileSize; }
		}
		public Color BackgroundColor {
			get { return backgroundColor; }
		}
		public TileLayer[] Layers {
			get { return layers; }
		}
	}
}