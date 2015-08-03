using UnityEngine;
using System.Collections.Generic;
using SA;

public class TileMapGrid : MonoBehaviour, IEnumerable<TileMapVisual> {
	[SerializeField]
	private GameObject tileMapVisualPrefab;
	[SerializeField]
	private Material tileMaterial;
	[SerializeField]
	private Size2i size;
	[SerializeField]
	private Size2i tileMapTileSize;

	private TileMapVisual[] grid;
	private Dictionary<int, Color32[]> lightmap;

	public void CreateGrid() {
		var oldRoot = transform.FindChild( "TileMapsRoot" );
		if( oldRoot != null ) {
			Destroy( oldRoot.gameObject );
			oldRoot = null;
		}
		
		var root = new GameObject( "TileMapsRoot" );
		root.transform.position = transform.position;
		root.transform.parent = transform;
		
		grid = new TileMapVisual[ size.width * size.height ];
		
		for( int i = 0; i < size.width * size.height; ++i ) {
			Vector2i tileMapPos = new Vector2i( i % size.width, i / size.width );
			var tileMapVisualGo = new GameObject( tileMapPos.ToString() );
			tileMapVisualGo.transform.parent = root.transform;

			Vector2 tileMapLocalPos = (Vector2)tileMapTileSize;
			tileMapLocalPos.Scale( (Vector2) tileMapPos );
			tileMapLocalPos = ( Constants.TILE_SIZE * tileMapLocalPos  ) / Constants.PIXELS_PER_UNIT;
			tileMapVisualGo.transform.localPosition = tileMapLocalPos;
			
			var tileMapVisual = tileMapVisualGo.AddComponent<TileMapVisual>();
			grid[ i ] = tileMapVisual;
			tileMapVisual.TileMaterial = tileMaterial;
		}

		lightmap = new Dictionary<int, Color32[]>();
	}

	public void SetTileMapAt( TileMap tileMap, TilesetLookup tilesetLookup, int x, int y ) {
		if( x < 0 || x >= size.width || y < 0 || y >= size.height ) {
			DebugUtil.LogError( "Trying to set tilemap outside bounds: " + size );
			return;
		}

		if( tileMap.Size != tileMapTileSize ) {
			DebugUtil.LogError( "Tilemap.Size " + tileMap.Size + " != tileMapSize " + tileMapTileSize );
			return;
		}

		int index = x + y * size.width;
		var tileMapVisual = grid[ index ];
		tileMapVisual.CreateWithTileMap( tileMap, tilesetLookup );
	}

	public TileMap TileMapAtWorldPos( Vector2 worldPos ) {
		return TileMapAtTilePos( WorldPosToTilePos( worldPos ) );
	}
	
	public TileMap TileMapAtTilePos( Vector2i tilePos ) {
		return TileMapAtTilePos( tilePos.x, tilePos.y );
	}

	public TileMap TileMapAtTilePos( int x, int y ) {
		var v = TileMapVisualAtTilePos( x, y );
		return v == null ? null : v.TileMap;
	}

	public TileMapVisual TileMapVisualAtTilePos( int x, int y ) {
		x /= tileMapTileSize.width;
		y /= tileMapTileSize.height;
		
		int index = x + y * size.width;
		if( index < 0 || index >= grid.Length ) {
			return null;
		}
		
		return grid[ index ];
	}

	public Recti TileMapTileBounds( TileMap tileMap ) {
		Vector2i tp = TileMapTilePos( tileMap );
		return new Recti( tp.x, tp.y, tileMapTileSize.width, tileMapTileSize.height );
	}

	private Vector2i TileMapTilePos( TileMap tileMap ) {
		Vector2i tileMapGridPos = TileMapGridPos( tileMap );
		tileMapGridPos.x *= tileMapTileSize.width;
		tileMapGridPos.y *= tileMapTileSize.height;
		return tileMapGridPos;
	}

	private Vector2i TileMapGridPos( TileMap tileMap ) {
		var tileMapGridPos = new Vector2i( 0, 0 );

		if( tileMap == null ) {
			return tileMapGridPos;
		}

		int index = System.Array.FindIndex( grid, ( visual ) => {
			return visual.TileMap == tileMap;
		} );

		if( index != -1 ) {
			tileMapGridPos = new Vector2i( index % size.width, index / size.width );
		}

		return tileMapGridPos;
	}

	public System.UInt32 TileAtTilePos( Vector2i tilePos ) {
		return TileAtTilePos( tilePos.x, tilePos.y );
	}

	public System.UInt32 TileAtTilePos( int x, int y ) {
		TileMap tileMap = TileMapAtTilePos( x, y );
		if( tileMap == null ) {
			return 0;
		}

		int gridPosX = (int)( x ) / tileMapTileSize.width;
		int gridPosY = (int)( y ) / tileMapTileSize.height;
		int tileMapTileX = gridPosX * tileMapTileSize.width;
		int tileMapTileY = gridPosY * tileMapTileSize.height;
		int localX = (int)x - tileMapTileX;
		int localY = (int)y - tileMapTileY;

		return Tile.UUID( tileMap.MidgroundLayer.Tiles[ localX + localY * tileMapTileSize.width ] );
	}

	public Vector2 TilePosToWorldPos( Vector2i tilePos ) {
		return new Vector2( (float)tilePos.x * Constants.TILE_SIZE_UNITS, (float)tilePos.y * Constants.TILE_SIZE_UNITS );
	}

	// TODO: Move this somewhere
	public Vector2i WorldPosToTilePos( Vector2 pos ) {
		return new Vector2i( Mathf.FloorToInt( ( pos.x * Constants.PIXELS_PER_UNIT ) / Constants.TILE_SIZE ),
		                     Mathf.FloorToInt( ( pos.y * Constants.PIXELS_PER_UNIT ) / Constants.TILE_SIZE ) );
	}

	public Vector2 LayerToWorldPos( TileMap tileMap, Layer layer, Vector2 layerPos ) {
		var tileMapGridPos = TileMapGridPos( tileMap );

		Vector2 tileMapLocalPos = (Vector2)tileMapGridPos;
		tileMapLocalPos.Scale( (Vector2)tileMapTileSize );
		tileMapLocalPos = ( Constants.TILE_SIZE * tileMapLocalPos  ) / Constants.PIXELS_PER_UNIT;

		layerPos.y = ( tileMapTileSize.height * Constants.TILE_SIZE ) - layerPos.y;
		layerPos /= Constants.PIXELS_PER_UNIT;

		return ( tileMapLocalPos + layerPos ) + (Vector2)transform.position;
	}

	// Iteration
	public IEnumerator<TileMapVisual> GetEnumerator() {
		int length = grid.Length;
		for( int i = 0; i < length; ++i ) {
			yield return grid[ i ];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	// Light Sources
	private void GetLightMapColorsAtGridIndex( int gridIndex, out Color32[] colors ) {
		if( lightmap.ContainsKey( gridIndex ) ) {
			colors = lightmap[ gridIndex ];
		} else {
			colors = new Color32[ tileMapTileSize.width * tileMapTileSize.height ];
			for( int i = 0; i < colors.Length; ++i ) {
				byte zero = (byte)0;
				var color = new Color32( zero, zero, zero, 255 );
				colors[ i ] = color;
			}
			lightmap[ gridIndex ] = colors;
		}
	}

	public void DoGlobalLight() {
		const int overshoot = 2;
		int maxY = ( grid.Length / size.width ) * tileMapTileSize.height - 1;
		int currentOvershoot = 0;

		for( int x = 0; x < size.width * tileMapTileSize.width; ++x ) {
			currentOvershoot = -1;

			for( int y = maxY; y >= 0; --y ) {
				int gridPosX = (int)( x ) / tileMapTileSize.width;
				int gridPosY = (int)( y ) / tileMapTileSize.height;
				int gridIndex = gridPosX + gridPosY * size.width;

				TileMapVisual tileMapVisual = grid[ gridIndex ];
				if( tileMapVisual.TileMap == null ) {
//					y -= tileMapTileSize.height;
					continue;
				}

				System.UInt32[] tiles = tileMapVisual.TileMap.MidgroundLayer.Tiles;

				int tileMapTileX = gridPosX * tileMapTileSize.width;
				int tileMapTileY = gridPosY * tileMapTileSize.height;
				int localX = (int)x - tileMapTileX;
				int localY = (int)y - tileMapTileY;
				int localIndex = localX + localY * tileMapTileSize.width;

				bool tileSolid = Tile.UUID( tiles[ localIndex ] ) != 0u;

				byte r = 255;
				byte g = 255;
				byte b = 255;
				byte a = 255;

				Color32[] colors;
				GetLightMapColorsAtGridIndex( gridIndex, out colors );

				if( tileSolid ) {
					colors[ localIndex ] = new Color32( r, g, b, a );
				} else {
					colors[ localIndex ] = new Color32( 0, 0, 0, 0 );
				}

				if( tileSolid && currentOvershoot == -1 ) {
					currentOvershoot = overshoot;
					continue;
				}

				if( currentOvershoot != -1 && ( --currentOvershoot == 0 ) ) {
					break;
				}
			}
		}
	}

	public void DoLightSource( Vector2i position, float lightRadius, Color lightColor, Easing.Mode lightMode = Easing.Mode.In, Easing.Algorithm lightAlgo = Easing.Algorithm.Linear ) {
		uint width = (uint)( size.width * tileMapTileSize.width );
		uint height = (uint)( size.height * tileMapTileSize.height );
		int lightX = position.x;
		int lightY = position.y;

		uint radius = (uint)Mathf.Max( lightRadius, 1 );
		Vector2i lightOrigin = new Vector2i( lightX, lightY );
		Vector2  lightOriginFloat = new Vector2( position.x, position.y );
		
		byte r = (byte)(lightColor.r * 255.0f * lightColor.a);
		byte g = (byte)(lightColor.g * 255.0f * lightColor.a);
		byte b = (byte)(lightColor.b * 255.0f * lightColor.a);

		for( int gridIndex = 0; gridIndex < grid.Length; ++gridIndex) {
			TileMapVisual tileMapVisual = grid[ gridIndex ];
			if( tileMapVisual == null || tileMapVisual.TileMap == null ) {
				continue;
			}

			Color32[] colors;
			GetLightMapColorsAtGridIndex( gridIndex, out colors );

			int posX = ( gridIndex % size.width ) * tileMapTileSize.width;
			int posY = ( gridIndex / size.width ) * tileMapTileSize.height;
			Recti bounds = new Recti( posX, posY, tileMapTileSize.width, tileMapTileSize.height );
			if( bounds.ContainsPoint( new Vector2i( lightX, lightY ) ) ) {
				int localX = lightX - posX;
				int localY = ( (int)lightY - posY ) ;
				int localIndex = localX + localY * tileMapTileSize.width;
				System.UInt32 tile = Tile.UUID( tileMapVisual.TileMap.MidgroundLayer.Tiles[ localIndex ] );

				colors[ localIndex ] = new Color32( r, g, b, tile == 0u ? (byte)0 : (byte)255 );
			}
		}
		
		SA.FieldOfView.LightenPoint(lightOrigin, radius, 3u, width, height,(x, y) => {
			return TileAtTilePos((int)x, (int)y) == 0u ? false : true;
		}, (x, y, visible) => {
			int gridPosX = (int)( x ) / tileMapTileSize.width;
			int gridPosY = (int)( y ) / tileMapTileSize.height;
			int gridIndex = gridPosX + gridPosY * size.width;

			if( gridIndex < 0 || gridIndex > grid.Length ) {
				return;
			}

			TileMapVisual tileMapVisual = grid[ gridIndex ];

			if( tileMapVisual.TileMap == null ) {
				return;
			}

			float f = ((float)(Vector2.Distance(lightOriginFloat, new Vector2((int)x,(int)y))) / (float) radius);
			f = Easing.Alpha(f, lightMode, lightAlgo);

			// Ensured to exist at this point
			Color32[] colors = lightmap[ gridIndex ];

			int tileMapTileX = gridPosX * tileMapTileSize.width;
			int tileMapTileY = gridPosY * tileMapTileSize.height;
			int localX = (int)x - tileMapTileX;
			int localY = (int)y - tileMapTileY;
			int localIndex = localX + localY * tileMapTileSize.width;

			System.UInt32 tile = Tile.UUID( tileMapVisual.TileMap.MidgroundLayer.Tiles[ localIndex ] );

			bool tileSolid = tile > 0u;

			byte r2 = r;
			byte g2 = g;
			byte b2 = b;
			byte a = 255;
			
			if( tileSolid ) {
				colors[ localIndex ] = Color32.Lerp(new Color32( r2, g2, b2, a ), new Color32( 0, 0, 0, 255 ), f );
			} else {
				a = (byte)( 255.0f * f );	
				
				colors[ localIndex ] = new Color32( 0, 0, 0, a );
			}
		});
	}

	public void ApplyLightMap() {
		if( lightmap == null ) {
			return;
		}

		foreach( var kvp in lightmap ) {
			var tileMapVisual = grid[ kvp.Key ];
			foreach( Transform child in tileMapVisual.transform ) {
				if( child.gameObject.name != "Midground" ) {
					continue;
				}
				var meshTiles = child.GetComponent<MeshTiles>();
				meshTiles.TileColors = kvp.Value;
			}
		}

		lightmap.Clear();
	}

	private Color32 Blend( Color32 c1, Color32 c2 ) {
		byte r = (byte)((((int)c1.r) + ((int)c2.r)));
		byte g = (byte)((((int)c1.g) + ((int)c2.b)));
		byte b = (byte)((((int)c1.b) + ((int)c2.b)));
		byte a = (byte)((((int)c1.a) + ((int)c2.a)));
		return new Color32( r, g, b, a );
	}
}
