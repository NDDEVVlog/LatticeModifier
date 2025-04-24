Shader "Unlit/FFDShader_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  
        _NormalMap ("Normal Map", 2D) = "bump" {}  
        _MetallicMap ("Metallic Map", 2D) = "white" {}  
        _AOMap ("Ambient Occlusion Map", 2D) = "white" {}  
        _Metallic ("Metallic", Range(0,1)) = 0.5  
        _Smoothness ("Smoothness", Range(0,1)) = 0.5  
        _MaskColor ("MaskColor", Color) = (1,0,0,1)  
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 texcoord3 : TEXCOORD3;  
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            sampler2D _MetallicMap;
            sampler2D _AOMap;

            float _Metallic;
            float _Smoothness;
            float4 _MaskColor;

            v2f vert (appdata v)
            {
                v2f o;

                bool isMasked = all(abs(v.color.rgb - _MaskColor.rgb) < 0.01);
                float3 newPos = isMasked ? v.texcoord3 : v.vertex.xyz;

                o.vertex = UnityObjectToClipPos(float4(newPos, 1.0));
                o.uv = v.uv;
                o.color = v.color;

                // **Correct normal transformation**
                float3 worldNormal = normalize(mul(v.normal, (float3x3)UNITY_MATRIX_IT_MV));
                o.normal = isMasked ? worldNormal : v.normal.xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample base texture
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a == 0) col = fixed4(1, 1, 1, 1); // Fallback color if no texture

                // **Metallic & Smoothness**
                float metallic = _Metallic;
                float smoothness = _Smoothness;

                // Check if the Metallic Map is assigned by sampling the texture and checking if it contains meaningful data (like non-zero value)
                fixed4 metallicTex = tex2D(_MetallicMap, i.uv);
                if (metallicTex.r > 0.0 || metallicTex.g > 0.0) // Check if the metallic map has a non-zero value
                {
                    metallic = metallicTex.r * _Metallic;
                    smoothness = metallicTex.g * _Smoothness;
                }

                // **AO (darken texture if available)**
                float ao = 1.0;
                fixed4 aoTex = tex2D(_AOMap, i.uv);
                if (aoTex.r > 0.0) // If the AO map has a meaningful value
                {
                    ao = aoTex.r;
                }
                col.rgb *= ao;

                return col;
            }
            ENDCG
        }
    }
}
