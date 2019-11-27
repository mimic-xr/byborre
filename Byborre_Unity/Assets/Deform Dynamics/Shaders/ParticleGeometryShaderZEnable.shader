Shader "Deform Dynamics/Particle Geometry Shader Z Enable" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_Size("_Size", float) = 0.01
	}

		SubShader{
		Tags{ "LightMode" = "ForwardBase" }
		Cull Off

		Pass{
		CGPROGRAM
#include "UnityCG.cginc"
#pragma target 4.6 
#pragma vertex vertex_shader
#pragma fragment fragment_shader
#pragma geometry geom

		struct Point {
		float3 vertex;
		float3 color;
	};

	StructuredBuffer<Point> points;

	struct v2g {
		float4 vertex : SV_POSITION;
		float4 color : COLOR;
	};

	struct g2f {
		float4 vertex : SV_POSITION;
		float4 color : COLOR;
	};

	float _Size;

	v2g vertex_shader(uint id : SV_VertexID, uint inst : SV_InstanceID)
	{
		v2g o;

		o.vertex = float4(points[id].vertex, 1);
		o.color = float4(points[id].color, 1);

		return o;
	}

	[maxvertexcount(36)]
	void geom(point v2g input[1], inout TriangleStream<g2f> tristream)
	{
		g2f o;

		float4 p = input[0].vertex;

		o.color = input[0].color;

		float4 p0 = p - float4(-_Size, -_Size, +_Size, 0);
		float4 p1 = p - float4(+_Size, -_Size, +_Size, 0);
		float4 p2 = p - float4(+_Size, -_Size, -_Size, 0);
		float4 p3 = p - float4(-_Size, -_Size, -_Size, 0);

		float4 p4 = p - float4(-_Size, +_Size, +_Size, 0);
		float4 p5 = p - float4(+_Size, +_Size, +_Size, 0);
		float4 p6 = p - float4(+_Size, +_Size, -_Size, 0);
		float4 p7 = p - float4(-_Size, +_Size, -_Size, 0);

		o.vertex = p0; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p3; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p1; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p1; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p2; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p3; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p4; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p7; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p5; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p5; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p6; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p7; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p0; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p4; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p1; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p1; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p4; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p5; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p3; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p7; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p2; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p2; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p6; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p7; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p0; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p3; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p4; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p3; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p4; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p7; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p1; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p2; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p5; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		tristream.RestartStrip();

		o.vertex = p2; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p5; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
		o.vertex = p6; o.vertex = mul(UNITY_MATRIX_VP, o.vertex); tristream.Append(o);
	}

	fixed4 fragment_shader(g2f i) : SV_Target
	{
		return fixed4(i.color.x, i.color.y, i.color.z, i.color.w);
	}

		ENDCG
	}
	}
}