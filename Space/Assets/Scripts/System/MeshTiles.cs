using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshTiles : MonoBehaviour 
{
	public struct SpriteData {
		public readonly Sprite Sprite;
		public readonly bool FlippedHori;
		public readonly bool FlippedVert;
		public readonly bool FlippedDiag;

		public SpriteData( Sprite sprite, bool hori, bool vert, bool diag ) {
			Sprite = sprite;
			FlippedHori = hori;
			FlippedVert = vert;
			FlippedDiag = diag;
		}
	}

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

	public delegate SpriteData SpriteAtHandler(uint x, uint y);
	public SpriteAtHandler SpriteAt;

	private const int kMaxVerticesInMesh = 65000;

	private float meshSize;

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
			var spriteData = SpriteAt( (uint)p.x, (uint)p.y );
			baseSprite = spriteData.Sprite;
			++p.x;
			if( p.x >= (int)width ) {
				p.x = 0;
				++p.y;
			}
		} while( baseSprite == null && p.x < width && p.y < height );

		if( baseSprite == null ) {
			DebugUtil.LogWarn( "Empty tile layer" );
			yield break;
		}

		Texture2D textureAtlas = baseSprite.texture;

		TileMaterial.mainTexture = textureAtlas;
		
		meshRenderer.material = TileMaterial;
		
		/* * * * * * * * * * * * * * * * * */
		
		meshSize = baseSprite.textureRect.width / baseSprite.pixelsPerUnit;
		
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
		for( uint i = 0; i < numTiles; ++i ) {			
			bool removeOldColliders = false;
			UpdateTile( i, ref vertices, ref triangles, ref uvs, createColliders, removeOldColliders, width, meshSize, collidersRoot );
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

	public bool UpdateTileAt( uint x, uint y, bool createCollider ) {
		if( isLoading ) {
			return false;
		}

		uint index = x + y * Width;

		Vector3[] vertices = meshFilter.mesh.vertices;
		int[] triangles = meshFilter.mesh.triangles;
		Vector2[] uvs = meshFilter.mesh.uv;

		var oldColliders = transform.FindChild( "__colliders" );
		bool removeOldCollider = true;
		UpdateTile( index, ref vertices, ref triangles, ref uvs, createCollider, removeOldCollider, Width, meshSize, oldColliders.gameObject );

		Color32[] meshColors = meshFilter.mesh.colors32;
		for( int z = 0; z < 4; ++z ) {
			meshColors[ index * 4 + z] = new Color32( 0, 0, 0, 0 );
		}
		meshFilter.mesh.colors32 = meshColors;

		meshFilter.mesh.vertices = vertices;
		meshFilter.mesh.triangles = triangles;
		meshFilter.mesh.uv = uvs;

		return true;
	}

	private void UpdateTile( uint index, ref Vector3[] vertices, ref int[] triangles, ref Vector2[] uvs,
	                         bool createCollider, bool removeOldCollider, uint width, float meshSize, GameObject collidersRoot ) {
		uint x = index % width;
		uint y = index / width;

		string colliderName = string.Format( "__collider{0}_{1}", x, y );

		if( removeOldCollider ) {
			var oldColl = collidersRoot.transform.FindChild( colliderName );
			if( oldColl != null ) {
				Destroy( oldColl.gameObject );
			}
		}

		float minX = meshSize*x;
		float minY = meshSize*y;
		float maxX = minX + meshSize;
		float maxY = minY + meshSize;
		
		uint vertexIndex = index * 4u;
		
		int bottomRightVertex = (int)vertexIndex;
		int bottomLeftVertex  = (int)vertexIndex + 1;
		int topRightVertex    = (int)vertexIndex + 2;
		int topLeftVertex     = (int)vertexIndex + 3;

		SpriteData spriteData = SpriteAt( x, y );

		if( !spriteData.FlippedDiag ) {
			vertices[bottomRightVertex] = new Vector3(maxX, maxY, 0.0f);
			vertices[bottomLeftVertex]  = new Vector3(minX, maxY, 0.0f);
			vertices[topRightVertex]    = new Vector3(maxX, minY, 0.0f);
			vertices[topLeftVertex]     = new Vector3(minX, minY, 0.0f);
		} else {
			vertices[bottomRightVertex] = new Vector3(minX, maxY, 0.0f);
			vertices[bottomLeftVertex]  = new Vector3(minX, minY, 0.0f);
			vertices[topRightVertex]    = new Vector3(maxX, maxY, 0.0f);
			vertices[topLeftVertex]     = new Vector3(maxX, minY, 0.0f);
		}
		
		Rect texRect = new Rect(0, 0, 0, 0);
		if( spriteData.Sprite != null ) {
			float texWidth = (float)spriteData.Sprite.texture.width;
			float texHeight = (float)spriteData.Sprite.texture.height;
			texRect = spriteData.Sprite.textureRect;
			texRect.x /= texWidth;
			texRect.y /= texHeight;
			texRect.width /= texWidth;
			texRect.height /= texHeight;

			if( createCollider ) {
				var collGo = new GameObject( colliderName );
				collGo.transform.parent = collidersRoot.transform;
				var boxCollider = collGo.AddComponent<BoxCollider2D>();
				boxCollider.transform.position = (Vector2)transform.position + new Vector2(minX, minY);
				boxCollider.offset = new Vector2( meshSize / 2.0f, meshSize / 2.0f );
				boxCollider.size = new Vector2( meshSize, meshSize );
			}
		}

		bool flipHori = false;
		bool flipVert = false;

		if( spriteData.FlippedHori ) {
			flipHori = !spriteData.FlippedDiag;
			flipVert = spriteData.FlippedDiag;
		}
		
		if( spriteData.FlippedVert ) {
			flipVert = !spriteData.FlippedDiag;
			flipHori = spriteData.FlippedDiag;
		}

		if( flipHori ) {
			float xMin = texRect.xMin;
			float xMax = texRect.xMax;
			texRect.xMin = xMax;
			texRect.xMax = xMin;
		}

		if( flipVert ) {
			float yMin = texRect.yMin;
			float yMax = texRect.yMax;
			texRect.yMin = yMax;
			texRect.yMax = yMin;
		}
		
		// Since Tiled renders with negative y, flip axis
		uvs[bottomRightVertex] = new Vector2(texRect.xMax, texRect.yMax);
		uvs[bottomLeftVertex] = new Vector2(texRect.xMin, texRect.yMax);
		uvs[topRightVertex] = new Vector2(texRect.xMax, texRect.yMin);
		uvs[topLeftVertex] = new Vector2(texRect.xMin, texRect.yMin);
		
		int triangleIndex = (int)index * 6;
		
		triangles[triangleIndex] = bottomRightVertex;
		triangles[triangleIndex+1u] = bottomLeftVertex;
		triangles[triangleIndex+2u] = topRightVertex;
		triangles[triangleIndex+3u] = bottomLeftVertex;
		triangles[triangleIndex+4u] = topLeftVertex;
		triangles[triangleIndex+5u] = topRightVertex;
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
