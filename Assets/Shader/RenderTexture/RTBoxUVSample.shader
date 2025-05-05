Shader "Custom/RTBoxUVSample"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
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

            sampler2D _MainTex;
            sampler2D _GlobalRenderTex;

            float3 _BoxCenter;
            float3 _BoxRight;
            float3 _BoxUp;

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata_full v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 delta = i.worldPos - _BoxCenter;

                float u = dot(delta, normalize(_BoxRight)) / length(_BoxRight) + 0.5;
                float v = dot(delta, normalize(_BoxUp)) / length(_BoxUp) + 0.5;

                if (u < 0 || u > 1 || v < 0 || v > 1)
                    return tex2D(_MainTex, float2(0.5, 0.5)); // fallback

                return tex2D(_GlobalRenderTex, float2(u, v));
            }
            ENDCG
        }
    }
}
