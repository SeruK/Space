using UnityEngine;
using SA;

public class TileMapVisual : MonoBehaviour {
	[SerializeField]
	private MeshTiles meshTiles;

	private TileMap tileMap;
	private TilesetLookup tilesetLookup;

	public void CreateWithTileMap( TileMap tileMap, TilesetLookup tilesetLookup ) {
		this.tileMap = tileMap;
		this.tilesetLookup = tilesetLookup;

		meshTiles.Width = (uint)tileMap.Size.width;
		meshTiles.Height = (uint)tileMap.Size.height;
		meshTiles.SpriteAt = (x, y) => {
			System.UInt32 tile = tileMap.MidgroundLayer.Tiles[x + y * tileMap.Size.width];
			return tilesetLookup.Tiles[ (int)tile ].TileSprite;
		};
		meshTiles.StartGeneratingMeshes();
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
		
		SA.FieldOfView.LightenPoint(lightOrigin, radius, 3u, width, height,(x, y) => {
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
		
		meshTiles.TileColors = colors;
	}

	private System.UInt32 TileAt( uint x, uint y ) {
		return tileMap.MidgroundLayer.Tiles[ x + y * tileMap.Size.width ];
	}
}
