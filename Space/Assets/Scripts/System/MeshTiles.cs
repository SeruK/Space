using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshTiles : MonoBehaviour 
{
	public Material TileMaterial;
	[HideInInspector]
	public uint Width;
	[HideInInspector]
	public uint Height;

	private MeshFilter meshFilter;
	
	private Color32[] tileColors;
	public Color32[] TileColors
	{
		get {
			return tileColors;
		}
		
		set {
			tileColors = value;
			if(isLoading || meshFilter == null || meshFilter.mesh == null)
			{
				return;
			}
			applyTileColors();
		}
	}
	
	private bool isLoading = false;
	private bool createColliders = false;

	public delegate Sprite SpriteAtHandler(uint x, uint y);
	public SpriteAtHandler SpriteAt;

	private const int kMaxVerticesInMesh = 65000;
	
	void OnDisable()
	{
		stopLoading();
	}	
	
	public void ClampSizeToMaxVertices(ref uint width, ref uint height)
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
	
	public void StartGeneratingMeshes( bool createColliders )
	{
		stopLoading();
		this.createColliders = createColliders;
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
		if( SpriteAt == null ) {
			Debug.LogWarning( "SpriteAt was not set, unable to generate meshes." ); 
			yield break;
		}
		
		/* * * * * * * * * * * * * * * * * */
		
		ClampSizeToMaxVertices(ref Width, ref Height);
		
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

		Sprite baseSprite = null;
		SA.Vector2i p = new SA.Vector2i( 0, 0 );
		do {
			baseSprite = SpriteAt( (uint)p.x, (uint)p.y );
			++p.x;
			if( p.x >= (int)width ) {
				p.x = 0;
				++p.y;
			}
		} while( baseSprite == null );

		Texture2D textureAtlas = baseSprite.texture;

		TileMaterial.mainTexture = textureAtlas;
		
		meshRenderer.material = TileMaterial;
		
		/* * * * * * * * * * * * * * * * * */
		
		float meshSize = baseSprite.textureRect.width / baseSprite.pixelsPerUnit;
		
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

		var oldColliders = transform.FindChild( "__colliders" );
		if( oldColliders != null ) Destroy( oldColliders.gameObject );

		GameObject collidersRoot = null;
		if( createColliders ) {
			collidersRoot= new GameObject( "__colliders" );
			collidersRoot.transform.parent = transform;
			collidersRoot.transform.position = transform.position;
			var collidersRootRigid = collidersRoot.AddComponent<Rigidbody2D>();
			collidersRootRigid.isKinematic = true;
		}
		for(uint i = 0; i < numTiles; ++i)
		{			
			uint x = i % width;
			uint y = i / width;
			
			float minX = meshSize*x;
			float minY = meshSize*(height-y);
			float maxX = minX + meshSize;
			float maxY = (meshSize*(height-y) + meshSize);

			uint vertexIndex = i*4u;
			
			int bottomRightVertex = (int)vertexIndex;
			int bottomLeftVertex  = (int)vertexIndex + 1;
			int topRightVertex    = (int)vertexIndex + 2;
			int topLeftVertex     = (int)vertexIndex + 3;

			vertices[bottomRightVertex] = new Vector3(maxX, maxY, 0.0f);
			vertices[bottomLeftVertex] = new Vector3(minX, maxY, 0.0f);
			vertices[topRightVertex] = new Vector3(maxX, minY, 0.0f);
			vertices[topLeftVertex] = new Vector3(minX, minY, 0.0f);

			Sprite sprite = SpriteAt( x, y );

			Rect texRect = new Rect(0, 0, 0, 0);
			if( sprite != null ) {
				float texWidth = (float)baseSprite.texture.width;
				float texHeight = (float)baseSprite.texture.height;
				texRect = sprite.textureRect;
				texRect.x /= texWidth;
				texRect.y /= texHeight;
				texRect.width /= texWidth;
				texRect.height /= texHeight;

				if( createColliders ) {
					var collGo = new GameObject( "__collider" );
					collGo.transform.parent = collidersRoot.transform;
					var boxCollider = collGo.AddComponent<BoxCollider2D>();
					boxCollider.transform.position = (Vector2)transform.position + new Vector2(minX, minY);
					boxCollider.offset = new Vector2( meshSize / 2.0f, meshSize / 2.0f );
					boxCollider.size = new Vector2( meshSize, meshSize );
				}
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
		
		mesh.Optimize();
		mesh.MarkDynamic();
		mesh.RecalculateNormals();
		
		meshFilter.mesh = mesh;
		
		isLoading = false;

		applyTileColors();
	}
	
	private void applyTileColors()
	{
		if( meshFilter == null || meshFilter.mesh == null || tileColors == null ) {
			return;
		}
		
		uint numVertices = Width * Height * 4u;
		var colors = new Color32[numVertices];
		
		if((uint)tileColors.Length*4u != numVertices)
		{
			Debug.LogWarning("Tile colors len * 4 ("+(tileColors.Length*4)+") != num vertices ("+numVertices+")");
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
		tileColors = null;
	}
}
