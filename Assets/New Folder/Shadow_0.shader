

Shader "Qjl/Qjl_Shadow_0" {
	Properties {
		_ShadowTex ("阴影纹理", 2D) = "gray" {}
		_FadeTex ("假纹理", 2D) = "white" 
		//_MainColor ("主颜色", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue"="Transparent" }
		Pass {
			ZWrite Off
			Fog { Color (1, 1, 1) }
			AlphaTest Greater 0
			//ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos:POSITION;
				float4 sproj:TEXCOORD0;
				float4 fproj:TEXCOORD1;
				float4 color:COLOR;
			};

			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			sampler2D _ShadowTex;
			sampler2D _FadeTex;
			//fixed4 _MainColor;

			v2f vert(float4 vertex:POSITION){
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.sproj = mul(unity_Projector, vertex);
				o.fproj = mul(unity_ProjectorClip, vertex);
				return o;
			}

			float4 frag(v2f i):COLOR{
				float4 c = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.sproj));
				float fade = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.sproj)).r;
				float fadeout = tex2Dproj(_FadeTex, UNITY_PROJ_COORD(i.fproj)).g;
				float4 result;
				//fixed4 col=_MainColor.rgba;
				result.rgb = lerp(fixed3(1,1,1), 1-c.a, fadeout)*1.3;
				result.rgb += 1-fade;
				result.a = 1;
				return result;
				//return c;
			}

			ENDCG
		}
	} 
	FallBack "Diffuse"
}
