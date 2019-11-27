// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Deform Dynamics/Two Sided Same Color" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_NormalBias("Normal Bias", float) = 0.01
		_CameraBias("Camera Bias", float) = 0.01
		
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
			fixed _Cutoff;
			float _NormalBias;
			float _CameraBias;

			v2f vert(appdata_full v)
			{
				v2f o;

				float4 objSpaceCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
				float4 n = float4(v.normal, 1.0);

				o.vertex = UnityObjectToClipPos(v.vertex + (_NormalBias * n) + (_CameraBias * normalize(objSpaceCamPos - v.vertex)));
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
			fixed _Cutoff;

			v2f vert(appdata_full v)
			{
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

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

						// compile shader into multiple variants, with and without shadows
						// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
						// shadow helper functions and macros
			#include "AutoLight.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
					fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				float4 pos : SV_POSITION;
			};

			float _NormalBias;
			float _CameraBias;

			v2f vert(appdata_base v)
			{
				v2f o;
				float4 objSpaceCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
				float4 n = float4(v.normal, 1.0);

				o.pos = UnityObjectToClipPos(v.vertex + (_NormalBias * n) + _CameraBias * normalize(objSpaceCamPos - v.vertex));
				o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(worldNormal,1));
				// compute shadows data
				TRANSFER_SHADOW(o)
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
				fixed shadow = SHADOW_ATTENUATION(i);
				// darken light's illumination with shadow, keep ambient intact
				fixed3 lighting = i.diff * shadow + i.ambient;
				col.rgb *= lighting;
				return col;
			}
			ENDCG
		}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows nolightmap vertex:vert
		#pragma shader_feature _USEMETALLICMAP_ON
		#pragma target 3.0
		#pragma glsl

		sampler2D _MainTex;
		sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;

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
		
		float _NormalBias;
		float _CameraBias;

		void vert(inout appdata_full v) {
			float4 objSpaceCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
			float4 n = float4(v.normal, 1.0);

			v.vertex = v.vertex + (_NormalBias * n) + (_CameraBias * normalize(objSpaceCamPos - v.vertex));
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
			
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