Shader "Unlit/MoveBlackVertices"
{
    Properties
    {
        _Speed ("Speed", Float) = 2.0
        _Amplitude ("Amplitude", Float) = 0.5
        _Threshold ("Threshold", Float) = 0.1 // Sensitivity for detecting black
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR; // Get vertex color
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float _Speed;
            float _Amplitude;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.color = v.color; // Pass vertex color to fragment shader

                // Calculate color intensity to detect black
                float intensity = length(v.color.rgb);

                // Use Step to check if the intensity is below the threshold (i.e., close to black)
                float isBlack = step(intensity, _Threshold); // 1 if black, 0 otherwise

                // Apply vertical movement only to black vertices
                float offset = sin(_Time.z * _Speed) * _Amplitude * isBlack;

                // Move the vertex up/down
                v.vertex.z += offset;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color; // Use vertex color as the output color
            }
            ENDCG
        }
    }
}

