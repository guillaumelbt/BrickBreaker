Shader "Custom/DrawBrick"
{
Properties
{
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
        //#include "Assets/[Tools]/Shaders/Include/Hammersley.cginc"
        
		#define ID_PER_PRIMITIVE 6

        struct v2f
        {
            float4 position : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };
        uniform uint _BrickX;
        uniform uint _BrickY;
        uint2 GetCorner(uint index)
        {
            return uint2(index >= 2 && index <= 4, index >= 1 && index <= 3);
        }

        struct BrickData
        {
            uint hp;
            uint damage;
            float4 color;
        };
        
        struct WorldData
        {
            float2 handlePos;
            float2 handleSize;

            float2 worldSize;

            uint lostBallCount;
        };

        uniform StructuredBuffer<WorldData> _WorldData;
        uniform StructuredBuffer<BrickData> _BrickData;
        uniform uint _BrickDataSize;

        v2f vert (uint id : SV_VertexID)
        {
            const uint quadIndex = id / ID_PER_PRIMITIVE;
            const uint vertexIndex = id % ID_PER_PRIMITIVE;

            float2 cornerHG = float2(-_WorldData[0].worldSize.x, _WorldData[0].worldSize.y);
            float2 cornerBD = float2(_WorldData[0].worldSize.x, 0);

            uint column = quadIndex % _BrickX;
            uint row = quadIndex / _BrickX;
            uint brickIndex = row * _BrickX + column;

            
            v2f o;
            if (row< _BrickY)
            {
                uint2 vertexCorner = uint2(column, row) + GetCorner(vertexIndex);
                float2 lerpFactor = vertexCorner / float2(_BrickX, _BrickY);
                float2 vertexPos2D = lerp(cornerHG, cornerBD, lerpFactor);

                const float2 quadPos = lerp(cornerHG, cornerBD, (0.5 + float2(column, row)) / float2(_BrickX, _BrickY));
                const float2 quadSize = 0.95 * (cornerHG - cornerBD) / float2(_BrickX, _BrickY);

                const float3 direction0 = float3(1, 0, 0);
                const float3 direction1 = float3(0, 1, 0);
                const float3 direction2 = float3(0, 0, 1);

                const float2 corner = GetCorner(vertexIndex) - 0.5f;
                const float3 localVertexPos = quadSize.x * corner.x * direction0 + 
                                            quadSize.y * corner.y * direction1;
                
                const float4 color = _BrickData[brickIndex].damage >= _BrickData[brickIndex].hp ? 0 : _BrickData[brickIndex].color;
                const float3 vertexPos = localVertexPos + float3(quadPos, 0);

                o.position = mul(UNITY_MATRIX_MVP, float4(vertexPos, 1));
                o.color = color;
                o.uv = GetCorner(vertexIndex);
            }
            else
            {
                o = (v2f)0;
            }
            
            return o;
        }

            
        float4 frag(v2f IN) : COLOR
        {
            return IN.color * 0.2;
        }
        ENDCG
    }
}
}
