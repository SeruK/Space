using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshTiles : MonoBehaviour 
{
	public Texture2D[] Textures;
	
	private Rect[] textureRects;
	
	public Material wholeTextureMaterial;
	
	public uint Width = 5;
	public uint Height = 5;
	
	public float TileSize = 2.0f;
	
	private MeshFilter meshFilter;
	
	private Color32[] tileColors;
	public Color32[] TileColors
	{
		get {
			return tileColors;
		}
		
		set {
			if(isLoading || meshFilter == null || meshFilter.mesh == null)
			{
				tileColors = value;
				return;
			}
			tileColors = null;
			applyTileColors(meshFilter.mesh, value);
		}
	}
	
	private bool isLoading = false;
	
	public delegate Rect TextureRectModification(uint x, uint y, Rect rect);
	public delegate uint TextureIndexHandler(uint x, uint y);
	
	public TextureRectModification OnAssignRect;
	public TextureIndexHandler TextureIndexForTile;
	
	private const int kMaxVerticesInMesh = 65000;
	
	void OnDisable()
	{
		stopLoading();
	}	
	
	public void ClampTileSizeToMaxVertices(ref uint width, ref uint height)
	{		
		while(((width+1u)*(height+1u)) >= kMaxVerticesInMesh)
		{
			if(width > height)
			{
				--width;
			}
			else if(width < height)
			{
				--height;
			} else
			{
				--width;
				--height;
			}
		}
	}
	
	public void StartGeneratingMeshes()
	{
		stopLoading();
		startLoading();
	}
	
	private void stopLoading()
	{
		isLoading = false;
		StopCoroutine("generateMeshes");
	}
	
	private void startLoading()
	{
		StartCoroutine("generateMeshes");	
	}
	
	private IEnumerator generateMeshes()
	{
		if(Textures == null || TextureIndexForTile == null)
		{
			Debug.LogWarning("Textures or TextureIndexForTile was not set, unable to generate meshes.");
			yield break;
		}
		
		/* * * * * * * * * * * * * * * * * */
		
		ClampTileSizeToMaxVertices(ref Width, ref Height);
		
		uint width = Width;
		uint height = Height;
		
		/* * * * * * * * * * * * * * * * * */
		
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		
		if(!meshFilter)
		{
			meshFilter = gameObject.AddComponent<MeshFilter>();
		}
		
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		
		if(!meshRenderer)
		{
			meshRenderer = gameObject.AddComponent<MeshRenderer>();	
		}
		
		/* * * * * * * * * * * * * * * * * */
		
		int texturesLength = Textures.Length;
		
		if(texturesLength % 2 != 0)
		{
			texturesLength += 1;	
		}

		bool mipMap = false;
		Texture2D textureAtlas = new Texture2D(0,0, TextureFormat.RGBA4444, mipMap);
		textureAtlas.wrapMode = TextureWrapMode.Clamp;
		textureRects = textureAtlas.PackTextures(Textures, 0);
		
		wholeTextureMaterial.mainTexture = textureAtlas;
		
		meshRenderer.material = wholeTextureMaterial;
		
		/* * * * * * * * * * * * * * * * * */
		
		float meshSize = TileSize;
		
		Mesh mesh = new Mesh();
		
		uint numTiles = Width*Height;
		uint numVertices = numTiles * 4u;
		uint numTriangles = numTiles * 6u;
		
		if(numVertices > kMaxVerticesInMesh)
		{
			Debug.LogError("Could not generate mesh, too many vertices. "+numVertices+"/"+kMaxVerticesInMesh);
			yield break;
		}
		
		this.meshFilter = meshFilter;
		isLoading = true;
		
		Vector3[] vertices = new Vector3[numVertices];
		int[] triangles = new int[numTriangles];
		Vector2[] uvs = new Vector2[numVertices];
		
//		Debug.Log("Tris: "+numTriangles/3u);
		
		float fullMeshHeight = meshSize * height;
		
		for(uint i = 0; i < numTiles; ++i)
		{			
			uint x = i % width;
			uint y = i / width;
			
			float minX = meshSize*x;
			float minY = fullMeshHeight - meshSize*(height-y);
			float maxX = minX + meshSize;
			float maxY = fullMeshHeight - (meshSize*(height-y) + meshSize);
			
			uint vertexIndex = i*4u;
			
			int bottomRightVertex = (int)vertexIndex;
			int bottomLeftVertex = (int)vertexIndex + 1;
			int topRightVertex = (int)vertexIndex + 2;
			int topLeftVertex = (int)vertexIndex + 3;
			
			vertices[bottomRightVertex] = new Vector3(maxX, maxY, 0.0f);
			vertices[bottomLeftVertex] = new Vector3(minX, maxY, 0.0f);
			vertices[topRightVertex] = new Vector3(maxX, minY, 0.0f);
			vertices[topLeftVertex] = new Vector3(minX, minY, 0.0f);
			
			Rect texRect = new Rect(textureRects[TextureIndexForTile(x, y)]);
			
			if(OnAssignRect != null)
			{
				texRect = OnAssignRect(x, y, texRect);
			}
			
			uvs[bottomRightVertex] = new Vector2(texRect.xMax, texRect.yMin);
			uvs[bottomLeftVertex] = new Vector2(texRect.xMin, texRect.yMin);
			uvs[topRightVertex] = new Vector2(texRect.xMax, texRect.yMax);
			uvs[topLeftVertex] = new Vector2(texRect.xMin, texRect.yMax);
			
			int triangleIndex = (int)i*6;
			
			triangles[triangleIndex] = bottomRightVertex;
			triangles[triangleIndex+1u] = bottomLeftVertex;
			triangles[triangleIndex+2u] = topRightVertex;
			triangles[triangleIndex+3u] = bottomLeftVertex;
			triangles[triangleIndex+4u] = topLeftVertex;
			triangles[triangleIndex+5u] = topRightVertex;
		}
		
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		
		applyTileColors(mesh, tileColors);
		tileColors = null;
		
		mesh.Optimize();
		mesh.MarkDynamic();
		mesh.RecalculateNormals();
		
		meshFilter.mesh = mesh;
		
		isLoading = false;
	}
	
	private void applyTileColors(Mesh mesh, Color32[] tileColors)
	{
		if(mesh == null || tileColors == null)
		{
			return;
		}
		
		uint numVertices = Width * Height * 4u;
		var colors = new Color32[numVertices];
		
		if((uint)tileColors.Length*4u != numVertices)
		{
			Debug.LogWarning("Tile colors len ("+tileColors.Length+") != num vertices ("+numVertices+")");
			return;	
		}
		
		for(int i = 0; i < numVertices; i+= 4)
		{
			var color = tileColors[i/4];
			for(int z = 0; z < 4; ++z)
			{
				colors[i+z] = color;
			}
		}
		
		meshFilter.mesh.colors32 = colors;
	}
}
