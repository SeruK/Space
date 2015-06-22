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
		tilePos.x /= tileMapTileSize.width;
		tilePos.y /= tileMapTileSize.height;
		
		int index = tilePos.x + tilePos.y * size.width;
		if( index < 0 || index >= tileMapVisuals.Length ) {
			return null;
		}
		
		return tileMapVisuals[ index ].TileMap;
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
}
