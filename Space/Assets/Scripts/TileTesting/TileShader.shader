// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/TileShader" {
Properties {
	_MainTex ( "Base (RGB) Trans (A)", 2D ) = "white" {}
	[MaterialToggle] PixelSnap ( "Pixel snap", Float ) = 0
	_LayerZ ( "Z layer", Float ) = 0
}

SubShader {
	Tags {
		"Queue"="Transparent"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
		"PreviewType"="Plane"
		"CanUseSpriteAtlas"="True"
	}
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Front
	Lighting Off
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4    _MainTex_ST;
			float     _LayerZ;
			
			v2f vert( appdata_t v ) {
				v2f OUT;
				OUT.vertex = mul( UNITY_MATRIX_MVP, v.vertex );
				OUT.vertex.x -= _WorldSpaceCameraPos.x * ( 0.0001 *_LayerZ );
				OUT.color = v.color;
				OUT.texcoord = TRANSFORM_TEX( v.texcoord, _MainTex );
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap( OUT.vertex );
				#endif
				return OUT;
			}
			
			fixed4 frag( v2f i ) : SV_Target {
				if( i.texcoord.x < 0.001 && i.texcoord.y < 0.001 ) {
					return i.color;
				}
				fixed4 col = tex2D( _MainTex, i.texcoord ) * i.color;
				return col;
			}
		ENDCG
	}
}

}
