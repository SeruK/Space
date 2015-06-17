using UnityEngine;
using System.Collections;
using SA;

public class SingleMeshBuggery : MonoBehaviour {
	
	public MeshTiles meshTiles;
	
	private uint width = 50;
	private uint height = 50;
	
	public int lightX = 10;
	public int lightY = 10;
	public int lightRadius = 10;
	
	public Color lightColor = Color.red;
	
	public Easing.Mode lightMode;
	public Easing.Algorithm lightAlgo;

	private TileMap tileMap;

	// Use this for initialization
	void OnEnable () 
	{
		string tmxFilePath = System.IO.Path.Combine( Application.streamingAssetsPath, "test.tmx" );
		tileMap = SA.TileMapTMXReader.ParseTMXFileAtPath( tmxFilePath );

		Camera.main.backgroundColor = tileMap.BackgroundColor;

		meshTiles.Width = (uint)tileMap.Size.width;
		meshTiles.Height = (uint)tileMap.Size.height;
		meshTiles.TextureIndexForTile = (x, y) => {
			return TileAt(x,y);
		};
//		meshTiles.TextureIndexForTile = (x, y) => {
//			return simplexAt(x, y) < 0.3f ? 0u : 1u;
//		};
		meshTiles.StartGeneratingMeshes();
		
		doLighten();
	}
	
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
		uint numVertices = width * height * 4u;
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
			
			byte r2 = r;
			byte g2 = g;
			byte b2 = b;
			byte a = 255;
			
			if(TileAt(x, y) > 0)
			{
				colors[i] = Color32.Lerp(new Color32(r2,g2,b2,a), new Color32(0,0,0,255), Easing.Alpha(f, lightMode, lightAlgo));
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
		return tile == 0u ? 0u : 1u;
	}

	float simplexAt(uint x, uint y)
	{
		float freq = 1.0f/(float)width;
		float lacunarity = 2.0f;
		float gain = 0.5f; // increases "noise", helps decrease the blockiness of it all
		float amplitude = 6.0f; // higher number decrease "thickness" of the paths created
//		float sensitivity = 0.3f;
		float f = SA.Simplex.GenerateOne2D((float)x, (float)y, freq, 3, lacunarity, gain, amplitude, true);
		return ((1.0f+f) / 2.0f);
	}
	
	void OnGUI()
	{
		GUILayout.Label("X: "+lightX + " Y: " + lightY);
	}
}
