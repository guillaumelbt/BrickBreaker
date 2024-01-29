Shader "Custom/GPUBrick"
{
Properties
{
    _BallSize("Ball size", Float) = 0.1
}
SubShader
{
    Tags
    {
        "RenderType" = "Geometry"
        "Queue" = "Geometry+0"
    }

    Pass
    {
        Blend One OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite On
        ZTest LEqual

        CGPROGRAM
        // UNITY_SHADER_NO_UPGRADE : disable unity upgrade
        #pragma target 5.0
        #pragma vertex vert
        #pragma fragment frag

        
		#define ID_PER_PRIMITIVE 6

        struct v2f
        {
            float4 position : POSITION;
            float2 uv : TEXCOORD0;
        };

        uint2 GetCorner(uint index)
        {
            return uint2(index >= 2 && index <= 4, index >= 1 && index <= 3);
        }

        struct BallData
        {
            float2 pos;
            float2 speed;
            uint status;
        };

        uniform float _BallSize;
        uniform StructuredBuffer<BallData> _BallData;
        uniform uint _BallDataSize;

        v2f vert (uint id : SV_VertexID)
        {
            const uint quadIndex = id / ID_PER_PRIMITIVE;
            const uint vertexIndex = id % ID_PER_PRIMITIVE;

            const float3 direction0 = float3(1, 0, 0);
            const float3 direction1 = float3(0, 1, 0);
			const float3 direction2 = float3(0, 0, 1);

            const float2 corner = GetCorner(vertexIndex) - 0.5f;
            const float2 quadSize = _BallData[quadIndex].status > 0 ? 0 :_BallSize;
            const float3 localVertexPos = quadSize.x * corner.x * direction0 + 
                                          quadSize.y * corner.y * direction1;
            const float3 quadPos = float3(_BallData[quadIndex].pos, 0); 

            const float3 vertexPos = localVertexPos + quadPos;

            v2f o;
            o.position = mul(UNITY_MATRIX_MVP, float4(vertexPos, 1));
            o.uv = 0.5f + corner;
            return o;
        }

            
        float4 frag(v2f IN) : COLOR
        {
            return 1;
        }
        ENDCG
    }
}
}
