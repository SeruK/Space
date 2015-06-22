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

	private TileMapVisual[] tileMapVisuals;

	public void CreateGrid() {
		var oldRoot = transform.FindChild( "TileMapsRoot" );
		if( oldRoot != null ) {
			Destroy( oldRoot.gameObject );
			oldRoot = null;
		}
		
		var root = new GameObject( "TileMapsRoot" );
		root.transform.position = transform.position;
		root.transform.parent = transform;
		
		tileMapVisuals = new TileMapVisual[ size.width * size.height ];
		
		for( int i = 0; i < size.width * size.height; ++i ) {
			Vector2i tileMapPos = new Vector2i( i % size.width, i / size.width );
			var tileMapVisualGo = new GameObject( tileMapPos.ToString() );
			tileMapVisualGo.transform.parent = root.transform;
			
			// TODO: Make this a constant somewhere
			const float TILE_SIZE_PIXELS = 20.0f;
			const float PIXELS_PER_UNIT = 20.0f;
			Vector2 tileMapLocalPos = (Vector2)tileMapTileSize;
			tileMapLocalPos.Scale( (Vector2) tileMapPos );
			tileMapLocalPos = ( TILE_SIZE_PIXELS * tileMapLocalPos  ) / PIXELS_PER_UNIT;
			tileMapVisualGo.transform.localPosition = tileMapLocalPos;
			
			var tileMapVisual = tileMapVisualGo.AddComponent<TileMapVisual>();
			tileMapVisuals[ i ] = tileMapVisual;
			tileMapVisual.TileMaterial = tileMaterial;
		}
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
		var tileMapVisual = tileMapVisuals[ index ];
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
		if( index < 0 || index >= tileMapVisuals.Length ) {
			return null;
		}
		
		return tileMapVisuals[ index ];
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

		int index = System.Array.FindIndex( tileMapVisuals, ( visual ) => {
			return visual.TileMap == tileMap;
		} );

		if( index != -1 ) {
			tileMapGridPos = new Vector2i( index % size.width, index / size.width );
		}

		return tileMapGridPos;
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

	// TODO: Move this somewhere
	private Vector2i WorldPosToTilePos( Vector2 pos ) {
		const float PIXELS_PER_UNIT = 20.0f;
		const float TILE_SIZE = 20.0f;
		return new Vector2i( Mathf.FloorToInt( ( pos.x * PIXELS_PER_UNIT ) / TILE_SIZE ),
		                     Mathf.FloorToInt( ( pos.y * PIXELS_PER_UNIT ) / TILE_SIZE ) );
	}

	public Vector2 LayerToWorldPos( TileMap tileMap, Layer layer, Vector2 layerPos ) {
		const float PIXELS_PER_UNIT = 20.0f;
		const float TILE_SIZE_PIXELS = 20.0f;

		var tileMapGridPos = TileMapGridPos( tileMap );

		Vector2 tileMapLocalPos = (Vector2)tileMapGridPos;
		tileMapLocalPos.Scale( (Vector2)tileMapTileSize );
		tileMapLocalPos = ( TILE_SIZE_PIXELS * tileMapLocalPos  ) / PIXELS_PER_UNIT;

		layerPos.y = ( tileMapTileSize.height * TILE_SIZE_PIXELS ) - layerPos.y;
		layerPos /= PIXELS_PER_UNIT;

		return ( tileMapLocalPos + layerPos ) + (Vector2)transform.position;
	}

	// Iteration
	public IEnumerator<TileMapVisual> GetEnumerator() {
		int length = tileMapVisuals.Length;
		for( int i = 0; i < length; ++i ) {
			yield return tileMapVisuals[ i ];
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}

	// Light Sources
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

		var tileMapColorLookup = new Dictionary<TileMapVisual, Color32[]>();
		
		for( int tileMapVisualIndex = 0; tileMapVisualIndex < tileMapVisuals.Length; ++tileMapVisualIndex) {
			TileMapVisual tileMapVisual = tileMapVisuals[ tileMapVisualIndex ];
			if( tileMapVisual == null || tileMapVisual.TileMap == null ) {
				continue;
			}

			Color32[] colors = new Color32[ tileMapTileSize.width * tileMapTileSize.height ];
			for( int i = 0; i < colors.Length; ++i )
			{
				byte zero = (byte)0;
				var color = new Color32( zero, zero, zero, 255 );
				
				colors[ i ] = color;
			}

			int posX = ( tileMapVisualIndex % size.width ) * tileMapTileSize.width;
			int posY = ( tileMapVisualIndex / size.width ) * tileMapTileSize.height;
			Recti bounds = new Recti( posX, posY, tileMapTileSize.width, tileMapTileSize.height );
			if( bounds.ContainsPoint( new Vector2i( lightX, lightY ) ) ) {
				int localX = lightX - posX;
				// Flipperoo
				int localY = tileMapTileSize.height - (int)lightY - posY;
				int localIndex = localX + localY * tileMapTileSize.width;
				System.UInt32 tile = Tile.UUID( tileMapVisual.TileMap.MidgroundLayer.Tiles[ localIndex ] );

				colors[ localIndex ] = new Color32( r, g, b, tile == 0u ? (byte)0 : (byte)255 );
			}

			tileMapColorLookup[ tileMapVisual ] = colors;
		}
		
		SA.FieldOfView.LightenPoint(lightOrigin, radius, 0u, width, height,(x, y) => {
			return TileAtTilePos((int)x, (int)y) == 0u ? false : true;
		}, (x, y, visible) => {
			int gridPosX = (int)( x ) / tileMapTileSize.width;
			int gridPosY = (int)( y ) / tileMapTileSize.height;
			int gridIndex = gridPosX + gridPosY * size.width;

			if( gridIndex < 0 || gridIndex > tileMapVisuals.Length ) {
				return;
			}

			TileMapVisual tileMapVisual = tileMapVisuals[ gridIndex ];

			if( tileMapVisual.TileMap == null ) {
				return;
			}

			float f = ((float)(Vector2.Distance(lightOriginFloat, new Vector2((int)x,(int)y))) / (float) radius);
			f = Easing.Alpha(f, lightMode, lightAlgo);

			Color32[] colors = tileMapColorLookup[ tileMapVisual ];

			int tileMapTileX = gridPosX * tileMapTileSize.width;
			int tileMapTileY = gridPosY * tileMapTileSize.height;
			int localX = (int)x - tileMapTileX;
			// Flipperoo
			int localY = tileMapTileSize.height - (int)y - tileMapTileY;
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

		foreach( var kvp in tileMapColorLookup ) {
			var tileMapVisual = kvp.Key;
			foreach( Transform child in tileMapVisual.transform ) {
				if( child.gameObject.name != "Midground" ) {
					continue;
				}
				var meshTiles = child.GetComponent<MeshTiles>();
				meshTiles.TileColors = kvp.Value;
			}
		}
	}
}
