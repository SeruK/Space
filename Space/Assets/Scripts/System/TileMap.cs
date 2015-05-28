using UnityEngine;

// Modeled after TMX-format
// http://doc.mapeditor.org/reference/tmx-map-format/#imagelayer

namespace SA {
	public class TileMapProperty {
		
	}

	public class TileMapObject {
		
	}

	public class TileMapObjectGroup {

	}

	public class Tileset {
		// The global ID that maps to the first tile
		// in this tileset
		private int    firstGID;

		// External tilesets
		//private string source

		// Maximum size of tiles
		private Size2i tileSize;
		// Spacing in pixels between tiles
		private int    spacing;
		// Offset in pixels
		private Vector2i tileOffset;

		public int FirstGID {
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

	}

	public class TileMap {
		private Size2i size;
		private Size2i tileSize;
		private Color  backgroundColor;

		public Size2i Size {
			get { return size; }
		}
		public Size2i TileSize {
			get { return tileSize; }
		}
		public Color BackgroundColor {
			get { return backgroundColor; }
		}
	}
}