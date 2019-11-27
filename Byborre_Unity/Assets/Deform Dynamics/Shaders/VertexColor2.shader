Shader "Custom/VertexColor2" {
	SubShader{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200

		Pass
		{
			ZWrite Off
		}

		CGPROGRAM
		#pragma surface surf NoLighting vertex:vert
		#pragma target 3.0


		struct Input {
			float4 vertColor;
		};

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = 0.5;
			return c;
		}

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.vertColor = v.color;			
		}

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.vertColor.rgb;
			o.Alpha = 0;//IN.vertColor.a;
		}
		ENDCG
	}
		FallBack "Diffuse"
}