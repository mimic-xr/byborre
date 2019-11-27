// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Deform Dynamics/Deform Shader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		
		_VertexTex("Vertex Modify", 2D) = "white" {}
		_NormalTex("Normal Modify", 2D) = "white" {}
		
		[Toggle] _UseMetallicMap("Use Metallic Map", Float) = 0.0
		[NoScaleOffset] _MetallicGlossMap("Metallic", 2D) = "black" {}
		[Gamma] _Metallic("Metallic", Range(0,1)) = 0.0
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_BumpScale("Scale", Float) = 1.0
		_Cutoff("Alpha Cutoff", Range(0.01,1)) = 0.5
	}

	SubShader{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200
		ZWrite Off
		Cull Off

		Pass 
		{
			ColorMask 0
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _VertexTex;
			sampler2D _NormalTex;
			fixed _Cutoff;

			v2f vert(appdata_full v)
			{
				float4 vertex = tex2Dlod(_VertexTex, float4(v.texcoord1.xy, 0, 0));
				float4 normal = tex2Dlod(_NormalTex, float4(v.texcoord1.xy, 0, 0));

				v.vertex = vertex;
				v.normal = normal;
				
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				clip(col.a - _Cutoff);
				return 0;
			}
		
			ENDCG
		}

		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				float2 texcoord : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _VertexTex;
			sampler2D _NormalTex;
			fixed _Cutoff;

			v2f vert(appdata_full v)
			{
				float4 vertex = tex2Dlod(_VertexTex, float4(v.texcoord1.xy, 0, 0));
				float4 normal = tex2Dlod(_NormalTex, float4(v.texcoord1.xy, 0, 0));

				v.vertex = vertex;
				v.normal = normal;

				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				o.texcoord = v.texcoord;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				clip(col.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
		
			ENDCG
		}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha:fade nolightmap vertex:vert
		#pragma shader_feature _USEMETALLICMAP_ON
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;

		sampler2D _VertexTex;
		sampler2D _NormalTex;

		float4 _MainTexure_ST;

		struct Input {
			float2 uv_MainTex;
			fixed facing : VFACE;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		half _BumpScale;
		fixed _Cutoff;

		void vert(inout appdata_full v) {
			float4 vertex = tex2Dlod(_VertexTex, float4(v.texcoord1.xy, 0, 0));
			float4 normal = tex2Dlod(_NormalTex, float4(v.texcoord1.xy, 0, 0));

			v.vertex = vertex;
			v.normal = normal;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

		#ifdef _USEMETALLICMAP_ON
			fixed4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
			o.Metallic = mg.r;
			o.Smoothness = mg.a;
		#else
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		#endif

			// Rescales the alpha on the blended pass
			o.Alpha = saturate(c.a / _Cutoff);

			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);

			if (IN.facing < 0.5)
				o.Normal *= -1.0;
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}