using UnityEngine;
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

	public void DoLightSource( Vector2i position, float lightRadius, Color lightColor, Easing.Mode lightMode = Easing.Mode.In, Easing.Algorithm lightAlgo = Easing.Algorithm.Linear ) {
		if( tileMap == null || tilesetLookup == null ) {
			return;
		}

		uint width = (uint)tileMap.Size.width;
		uint height = (uint)tileMap.Size.height;
		int lightX = Mathf.Clamp( position.x, 0, (int)width - 1 );
		int lightY = ((int)height - 1) - Mathf.Clamp( position.y, 0, (int)height - 1);
		
		uint numVertices = width * height;
		var colors = new Color32[numVertices];
		
		for(int i = 0; i < numVertices; ++i)
		{
			byte zero = (byte)0;
			var color = new Color32(zero,zero,zero,255);
			
			colors[i] = color;
		}
		
		byte r = (byte)(lightColor.r * 255.0f * lightColor.a);
		byte g = (byte)(lightColor.g * 255.0f * lightColor.a);
		byte b = (byte)(lightColor.b * 255.0f * lightColor.a);
		
		if(lightX >= 0 && lightY >= 0 && lightX < width && lightY < height)
		{
			byte a = 255;
			if(TileAt((uint)lightX, (uint)lightY) == 0u)
			{
				a = 0;
			}
			colors[lightX+lightY*width] = new Color32(r,g,b,a);
		}
		
		uint radius = (uint)Mathf.Max(lightRadius, 1);
		Vector2i lightOrigin = new Vector2i(lightX, lightY);
		Vector2 lightOriginFloat = new Vector2(lightX, lightY);
		
		SA.FieldOfView.LightenPoint(lightOrigin, radius, 0u, width, height,(x, y) => {
			return TileAt((uint)lightX, (uint)lightY) == 0u ? false : true;
		}, (x, y, visible) => {
			uint i = x + y * width;
			
			float f = ((float)(Vector2.Distance(lightOriginFloat, new Vector2((int)x,(int)y))) / (float) radius);
			f = Easing.Alpha(f, lightMode, lightAlgo);
			
			byte r2 = r;
			byte g2 = g;
			byte b2 = b;
			byte a = 255;
			
			if(TileAt(x, y) > 0u)
			{
				colors[i] = Color32.Lerp(new Color32(r2,g2,b2,a), new Color32(0,0,0,255), f);
			} else
			{
				a = (byte)(255.0f * f);	
				
				colors[i] = new Color32(0,0,0,a);
			}
		});

		foreach( Transform child in transform ) {
			if( child.gameObject.name != "Midground" ) {
				continue;
			}

			var meshTiles = child.GetComponent<MeshTiles>();
			meshTiles.TileColors = colors;
		}
	}

	private System.UInt32 TileAt( uint x, uint y ) {
		return Tile.UUID( tileMap.MidgroundLayer.Tiles[ x + y * tileMap.Size.width ] );
	}
}
