using UnityEngine;
using System.Collections;
using SA;

public class SingleMeshBuggery : MonoBehaviour {
	
	public MeshTiles meshTiles;
	
	public int lightX = 10;
	public int lightY = 10;
	public int lightRadius = 10;
	
	public Color lightColor = Color.red;
	
	public Easing.Mode lightMode;
	public Easing.Algorithm lightAlgo;

	private TileMap tileMap;
	private TilesetLookup tilesetLookup;

	// Use this for initialization

	void OnEnable () 
	{
		string tmxFilePath = System.IO.Path.Combine( Util.ResourcesPath, "test.tmx" );
		tilesetLookup = new TilesetLookup();
		tileMap = SA.TileMapTMXReader.ParseTMXFileAtPath( tmxFilePath, tilesetLookup );

//		DebugTileset();
//		DebugTiles();

		Camera.main.backgroundColor = tileMap.BackgroundColor;

		meshTiles.Width = (uint)tileMap.Size.width;
		meshTiles.Height = (uint)tileMap.Size.height;
		meshTiles.SpriteAt = (x, y) => {
			System.UInt32 tile = TileAt(x,y);
			return tilesetLookup.Tiles[ (int)tile ].TileSprite;
		};
//		meshTiles.TextureIndexForTile = (x, y) => {
//			return simplexAt(x, y) < 0.3f ? 0u : 1u;
//		};
		meshTiles.StartGeneratingMeshes();
		
		doLighten();
	}

//	void DebugTileset() {
//		var masterGo = GameObject.Find( "tilemapdunk" );
//		if( masterGo != null) Destroy( masterGo );
//		masterGo = new GameObject( "tilemapdunk" );
//		for( int i = 1; i < tilesetLookup.Tiles.Count; ++i ) {
//			var tileInfo = tilesetLookup.Tiles[ i ];
//			var go = new GameObject( tileInfo.UUID.ToString() );
//			var sr = go.AddComponent<SpriteRenderer>();
//			sr.sprite = tileInfo.TileSprite;
//			go.transform.parent = masterGo.transform;
//			
//			Vector2 gopos = sr.sprite.textureRect.position;
//			gopos.x /= sr.sprite.pixelsPerUnit;
//			gopos.y /= sr.sprite.pixelsPerUnit;
//			
//			go.transform.position = gopos;
//		}
//		masterGo.transform.localScale = new Vector3( 20, 20, 20 );
//	}

//	void DebugTiles() {
//		var masterGo = GameObject.Find( "debugtiles" );
//		if( masterGo != null) Destroy( masterGo );
//		masterGo = new GameObject( "debugtiles" );
//		for( int y = 0; y < tileMap.Size.height; ++y ) {
//			for( int x = 0; x < tileMap.Size.width; ++x ) {
//				uint t = TileAt((uint)x, (uint)y);
//				if( t == 0 ) continue;
//				var tileInfo = tilesetLookup.Tiles[ (int)t ];
//				var go = new GameObject( t.ToString() );
//				var sr = go.AddComponent<SpriteRenderer>();
//				sr.sprite = tileInfo.TileSprite;
//				go.transform.parent = masterGo.transform;
//				go.transform.position = new Vector2( (float)x * (20.0f / sr.sprite.pixelsPerUnit), (float)y * (20.0f / sr.sprite.pixelsPerUnit ) );
//			}
//		}
//	}
//	
	private int lastLightX = 0;
	private int lastLightY = 0;
	private int lastRadius = 0;
	
	// Update is called once per frame
	void Update () {
		
//		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);	
//			
//		Plane plane = new Plane(transform.TransformDirection(Vector3.forward), transform.position);
//		
//		float distance = 0; 
//		
//  		if (plane.Raycast(ray, out distance))
//		{
//			Vector3 point = ray.GetPoint(distance);
//			var vec = point - transform.position;
//			
//			lightX = Mathf.FloorToInt(vec.x);
//			lightY = Mathf.FloorToInt(vec.y);
//		}

		if(lightX != lastLightX || lightY != lastLightY || lightRadius != lastRadius)
		{
			lastLightX = lightX;
			lastLightY = lightY;
			lastRadius = lightRadius;
			
			doLighten();
		}
	}
	
	void doLighten()
	{
		uint width = (uint)tileMap.Size.width;
		uint height = (uint)tileMap.Size.height;
		lightX = Mathf.Clamp( lightX, 0, (int)width - 1 );
		lightY = Mathf.Clamp( lightY, 0, (int)height - 1);

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

	uint TileAt(uint x, uint y)
	{
		var tile = tileMap.TileLayers[0].Tiles[x + (tileMap.Size.height-1-y) * tileMap.Size.width];
		return tile;
	}

//
//	float simplexAt(uint x, uint y)
//	{
//		float freq = 1.0f/(float)width;
//		float lacunarity = 2.0f;
//		float gain = 0.5f; // increases "noise", helps decrease the blockiness of it all
//		float amplitude = 6.0f; // higher number decrease "thickness" of the paths created
////		float sensitivity = 0.3f;
//		float f = SA.Simplex.GenerateOne2D((float)x, (float)y, freq, 3, lacunarity, gain, amplitude, true);
//		return ((1.0f+f) / 2.0f);
//	}
	
	void OnGUI()
	{
		GUILayout.Label("X: "+lightX + " Y: " + lightY);
	}
}
