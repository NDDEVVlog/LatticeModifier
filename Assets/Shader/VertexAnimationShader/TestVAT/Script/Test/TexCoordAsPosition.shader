    Shader "Custom/SimpleTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Texture property
        _Scale ("Scale", Float) = 1.0  // Define a float property for scaling
        _UseUV3 ("Use UV3 Offset", float) = 0 // 0 = false, 1 = true
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;   // Standard UVs for texture
                float3 texcoord3 : TEXCOORD3;  // Read from UV3
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex; // Texture sampler
            float _Scale;       // Declare Scale property
            float _UseUV3;

            v2f vert (appdata v)
            {
                v2f o;

                // Use texcoord3 if assigned, otherwise default to zero offset
                float3 offset = v.texcoord3;
                float isNonZero =_UseUV3 != 0  ? 1.0 : 0.0;
                float3 newPos = v.vertex.xyz + offset * isNonZero;


                o.vertex = UnityObjectToClipPos(float4(newPos, 1.0)); // Ensure correct transformation
                o.uv = v.uv; // Pass UVs to fragment shader

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv); // Sample and return texture color
            }
            ENDCG
        }
    }
}
