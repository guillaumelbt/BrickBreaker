Shader "Custom/DisplayWorld"
{
Properties
{
    _BorderWidth("Width",Range(0,1)) = 0.3
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
        Blend Off
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
            float4 color : COLOR;
        };

        uint2 GetCorner(uint index)
        {
            return uint2(index >= 2 && index <= 4, index >= 1 && index <= 3);
        }

        struct WorldData
        {
            float2 handlePos;
            float2 handleSize;

            float2 worldSize;

            uint lostBallCount;
        };

        uniform StructuredBuffer<WorldData> _WorldData;
        uniform uint _BallDataSize;
        uniform float _BorderWidth;

        v2f vert (uint id : SV_VertexID)
        {
            const uint quadIndex = id / ID_PER_PRIMITIVE;
            const uint vertexIndex = id % ID_PER_PRIMITIVE;

            const float3 direction0 = float3(1, 0, 0);
            const float3 direction1 = float3(0, 1, 0);
			const float3 direction2 = float3(0, 0, 1);

            const float2 corner = GetCorner(vertexIndex) - 0.5;
            float2 quadSize = 0;
            float3 quadPos = 0;
            float4 color = 0;
            
            
            if (quadIndex <= 2)
            {
                if(quadIndex == 0)
                {
                    quadPos = float3(-_WorldData[0].worldSize.x-(_BorderWidth/2),0,0);
                    quadSize = float2(_BorderWidth,_WorldData[0].worldSize.y*2);
                }
                else if(quadIndex == 1)
                {

                    quadPos = float3(0,_WorldData[0].worldSize.y+(_BorderWidth/2),0);
                    quadSize = float2(_WorldData[0].worldSize.x*2 + _BorderWidth*2,_BorderWidth);
                }
                else
                {
                    quadPos = float3(_WorldData[0].worldSize.x+(_BorderWidth/2),0,0);
                    quadSize = float2(_BorderWidth,_WorldData[0].worldSize.y*2);
                }
            }
            else if (quadIndex == 4)
            {
                quadSize = _WorldData[0].handleSize.yx;
                quadPos = float3(_WorldData[0].handlePos,0);
                color = float4(1,0,0,1);
                
            }
            else if (quadIndex == 5)
            {
                quadSize = float2(_WorldData[0].lostBallCount > 10 ? 0 : 10-_WorldData[0].lostBallCount,0.2);
                quadPos = float3(0,_WorldData[0].worldSize.y + 0.5,0);
                color = float4(1,1,0,1);
            }

            const float3 localVertexPos = quadSize.x * corner.x * direction0 + 
                                          quadSize.y * corner.y * direction1;
            const float3 vertexPos = localVertexPos + quadPos;

            v2f o;
            o.position = mul(UNITY_MATRIX_MVP, float4(vertexPos, 1));
            o.uv = 0.5f + corner;
            o.color = color;
            return o;
        }
            
        float4 frag (v2f IN) : COLOR
        {
            return IN.color;
        }
        ENDCG
    }
}
}
