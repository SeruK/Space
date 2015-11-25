using UnityEngine;
using SA;
using System;
using System.IO;

public class WorldGenerator {
	// TODO: Not this
	private static readonly UInt32[] DIRT_TILES = new UInt32[] { 16u, 10u, 15u };
	private static readonly UInt32[] DIRT_WEIGHTS = new uint[] { 8, 1, 1 };

	private RandomizerXor128 randomizer;

	private TileMapGrid tileMapGrid;
	private TilesetLookup tilesetLookup;
	private EntityManager entityManager;
	private Vector2 spawnPos;

	public Vector2 SpawnPos {
		get { return spawnPos; }
	}

	public WorldGenerator( TileMapGrid tileMapGrid, TilesetLookup tilesetLookup, EntityManager entityManager ) {
		this.randomizer = new RandomizerXor128();
		this.tileMapGrid = tileMapGrid;
		this.tilesetLookup = tilesetLookup;
		this.entityManager = entityManager;
	}

	public void GenerateTileGrid() {
		// TODO: Not this!
		string tmxFilePath = Path.Combine( Util.ResourcesPath, "test.tmx" );
		TileMap tileMap = SA.TileMapTMXReader.ParseTMXFileAtPath( tmxFilePath, tilesetLookup );

		var midgroundTiles = tileMap.MidgroundLayer.Tiles;
		var backgroundTiles = Array.Find( tileMap.TileLayers, ( TileLayer layer ) => { return layer.Name == "Background"; } ).Tiles;
		GenerateMountainTiles( ref midgroundTiles, ref backgroundTiles, 3, 3, 30, 30 );
		SetTileMapAt( tileMap, 3, 3 );
		GenerateTileMapAt( 1, 3 );
		GenerateTileMapAt( 2, 3 );
		GenerateTileMapAt( 4, 3 );
		GenerateTileMapAt( 5, 3 );
		// Dungeons
		GenerateTileMapAt( 1, 2 );
		GenerateTileMapAt( 2, 2 );
		GenerateTileMapAt( 3, 2 );
		GenerateTileMapAt( 4, 2 );
		GenerateTileMapAt( 5, 2 );
	}

	private void SetTileMapAt( TileMap tileMap, int x, int y ) {
		tileMapGrid.SetTileMapAt( tileMap, tilesetLookup, x, y );
		CreateTileMapObjects( tileMap );
	}
	
	private void GenerateTileMapAt( int gridX, int gridY ) {
		int w = 30; int h = 30;
		var tiles = new UInt32[ w * h ];
		var bgTiles = new UInt32[ w * h ];
		
		if( gridY < 3 ) {
			GenerateDungeonTiles( ref tiles, ref bgTiles, gridX, gridY, w, h );
		} else {
			GenerateMountainTiles( ref tiles, ref bgTiles, gridX, gridY, w, h );
		}
		var tileLayer = new TileLayer( "Midground", 1.0f, true, null, tiles );
		var bgLayer = new TileLayer( "Background", 1.0f, true, null, bgTiles );
		var tileLayers = new TileLayer[] { bgLayer, tileLayer };
		int midgroundIndex = 1;
		var tileMap = new TileMap( new Size2i( w, h ), new Size2i( 20, 20 ), Color.clear, null, null, tileLayers, null, midgroundIndex );
		SetTileMapAt( tileMap, gridX, gridY );
	}

	private UInt32 RandomDirtTile() {
		UInt32 tile = SA.Random.WeightedInArray( DIRT_TILES, DIRT_WEIGHTS, randomizer.GetNext );
		Tile.SetFlippedDiag( ref tile, SA.Random.CoinToss( randomizer.GetNext ) );
		Tile.SetFlippedHori( ref tile, SA.Random.CoinToss( randomizer.GetNext ) );
		Tile.SetFlippedVert( ref tile, SA.Random.CoinToss( randomizer.GetNext ) );
		return tile;
	}

	private void GenerateMountainTiles( ref UInt32[] tiles, ref UInt32[] bgTiles, int gridX, int gridY, int w, int h ) {
		float freq = 0.1f;
		int octaves = 5;
		float lacunarity = 2.0f;//range(2.0f, 3.0f);
		float gain = 0.6f;//range(0.6f, 0.7f); // increases "noise", helps decrease the blockiness of it all
		float amplitude = 2.0f;//range(6.0f, 8.0f); // higher number decrease "thickness" of the paths created
		
		// Start from bottom, generate upwards until we hit something
		for( int x = 0; x < w; ++x ) {
			float offsetX = gridX * w + x;
			float a = Simplex.GenerateOne1D( offsetX, freq, octaves, lacunarity, gain, amplitude );
			int yHeight = Mathf.FloorToInt( ( h / 2 ) + ( h / 2 ) * a );
			yHeight = Mathf.Min( yHeight, h - 1 );
			for( int y = 0; y < yHeight; ++y ) {
				int tileIndex = x + y * w;
				if( Tile.UUID( tiles[ tileIndex ] ) != 0u ) {
					break;
				}
				tiles[ tileIndex ] = RandomDirtTile();
			}
			for( int y = 0; y < yHeight; ++y ) {
				int tileIndex = x + y * w;
				if( Tile.UUID( bgTiles[ tileIndex ] ) != 0u ) {
					break;
				}
				bgTiles[ tileIndex ] = 14u;
			}
		}
	}
	
	private void GenerateDungeonTiles( ref UInt32[] tiles, ref UInt32[] bgTiles, int gridX, int gridY, int w, int h ) {
		for( int i = 0; i < w * h; ++i ) {
			bgTiles[ i ] = 14u;
		}
		
		float freq = 0.1f;
		int octaves = 5;
		float lacunarity = 2.0f;//range(2.0f, 3.0f);
		float gain = 0.8f;//range(0.6f, 0.7f); // increases "noise", helps decrease the blockiness of it all
		float amplitude = 10.0f;//range(6.0f, 8.0f); // higher number decrease "thickness" of the paths created
		
		Vector2i offset = new Vector2i( gridX * w, gridY * h );
		
		// Start from bottom, generate upwards until we hit something
		for( int x = 0; x < w; ++x ) {
			for( int y = 0; y < h; ++y ) {
				int tileIndex = x + y * w;
				if( Tile.UUID( tiles[ tileIndex ] ) != 0u ) {
					continue;
				}
				
				float a = Simplex.GenerateOne2D( offset.x + x, offset.y + y, freq, octaves, lacunarity, gain, amplitude );
				a = ( 1.0f + a ) / 2.0f;
				tiles[ tileIndex ] = a < 0.5f ? 0u : RandomDirtTile();
			}
		}
	}

	private void CreateTileMapObjects( TileMap tileMap ) {
		if( tileMap.ObjectLayers == null ) {
			return;
		}
		
		foreach( var objectLayer in tileMap.ObjectLayers ) {
			foreach( var layerObject in objectLayer.Objects ) {
				Vector2 worldPosOffset = new Vector2( Constants.TILE_SIZE_UNITS / 2.0f, -Constants.TILE_SIZE_UNITS );
				Vector2 worldPos = tileMapGrid.LayerToWorldPos( tileMap, objectLayer, layerObject.Position ) + worldPosOffset;

				if( layerObject.ObjectType == "SpawnPoint" ) {
					spawnPos = worldPos;
				}
				
				if( layerObject.ObjectType == "Goomba" ) {
					var goomba = entityManager.Spawn<Unit>( "Goomba" );
					goomba.transform.position = worldPos;
				}
				
				if( layerObject.ObjectType == "Item" ) {
					var pickup = entityManager.Spawn<Pickup>( "Pickup" );
					pickup.transform.position = worldPos;
					if( layerObject.Properties.ContainsKey( "ItemType" ) ) {
						pickup.ItemType = Item.ItemTypeFromString( layerObject.Properties[ "ItemType" ] );
					}
				}
				
				if( layerObject.ObjectType == "Spike" ) {
					var obstacle = entityManager.Spawn<Obstacle>( "Spike" );
					var tileLock = tileMapGrid.WorldPosToTilePos( worldPos ) + new Vector2i( 0, -1 );
					obstacle.transform.position = worldPos;
					obstacle.LockToTiles( tileLock );
				}
			}
		}
	}
}
