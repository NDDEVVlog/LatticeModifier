Shader "Unlit/BendWithUV2"
{
    Properties
    {
        _BendStrength ("Bend Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _BendStrength;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 originalPos : TEXCOORD1; // Vị trí gốc của vertex
                float3 targetPos : TEXCOORD2;   // Vị trí target từ UV2
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;

                // Lấy vector từ vertex tới target
                float3 targetDir = normalize(v.targetPos - v.originalPos);

                // Dịch chuyển vertex theo hướng target
                v.vertex.xyz += targetDir * _BendStrength;

                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1); // Shader màu trắng
            }

            ENDCG
        }
    }
}