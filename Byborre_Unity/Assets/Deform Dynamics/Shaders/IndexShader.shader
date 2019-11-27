Shader "Unlit/VertexID"
{
	Properties
	{
		_NumVertices("Vertex count", Float) = 1
	}
	SubShader
	{
		Pass
	{

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 3.5

	float _NumVertices;

	struct v2f {
		fixed4 color : TEXCOORD0;
		float4 pos : SV_POSITION;
	};

	v2f vert(float4 vertex : POSITION, uint vid : SV_VertexID)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(vertex);
		float f = (float) vid;
		o.color = half4(f / _NumVertices, f / _NumVertices, f / _NumVertices, 1);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		return i.color;
	}
		ENDCG
	}
	}
}