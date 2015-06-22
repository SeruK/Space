﻿using UnityEngine;
using SA;

public class TileMapVisual : MonoBehaviour {
	public Material TileMaterial;

	public TileMap TileMap {
		get { return tileMap; }
	}

	private TileMap tileMap;
	private TilesetLookup tilesetLookup;

	public void CreateWithTileMap( TileMap tileMap, TilesetLookup tilesetLookup ) {
		this.tileMap = tileMap;
		this.tilesetLookup = tilesetLookup;

		foreach( Transform child in transform ) {
			Destroy( child.gameObject );
		}

		for( int layerIndex = 0; layerIndex < tileMap.TileLayers.Length; ++layerIndex ) {
			var tileLayer = tileMap.TileLayers[ layerIndex ];

			var child = new GameObject( tileLayer.Name );
			child.transform.position = transform.position;
			child.transform.parent = transform;

			var meshTiles = child.AddComponent<MeshTiles>();
			meshTiles.TileMaterial = TileMaterial;
			meshTiles.Width = (uint)tileMap.Size.width;
			meshTiles.Height = (uint)tileMap.Size.height;
			meshTiles.SpriteAt = (x, y) => {
				System.UInt32 tile = tileLayer.Tiles[ x + y * tileMap.Size.width ];
				System.UInt32 uuid = Tile.UUID( tile );
				Sprite sprite = tilesetLookup.Tiles[ (int)uuid ].TileSprite;
				var spriteData = new MeshTiles.SpriteData( sprite,
				                                           Tile.FlippedHori( tile ),
				                                           Tile.FlippedVert( tile ),
				                                           Tile.FlippedDiag( tile ) );
				
				return spriteData;
			};
			bool isMidground = tileLayer == tileMap.MidgroundLayer;
			bool createColliders = isMidground;
			meshTiles.StartGeneratingMeshes( createColliders );

			if( !isMidground ) {
				int MIDGROUND_RENDER_QUEUE = 2500;
				int midgroundDiff = layerIndex - tileMap.MidgroundLayerIndex;
				var renderer = meshTiles.GetComponent<MeshRenderer>();
				renderer.material.renderQueue = MIDGROUND_RENDER_QUEUE + midgroundDiff;
			}

			int numTiles = tileMap.Size.width * tileMap.Size.height;
			var colors = new Color32[ numTiles ];
			for( int i = 0; i < numTiles; ++i ) {
				var color = Tile.UUID( tileLayer.Tiles[ i ] ) == 0 ? new Color( 0, 0, 0, 0 ) : new Color( 1, 1, 1, 1 );
				colors[ i ] = color;
			}
			meshTiles.TileColors = colors;
		}
	}
}
