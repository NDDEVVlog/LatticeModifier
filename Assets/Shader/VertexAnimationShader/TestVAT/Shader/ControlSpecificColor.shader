Shader "Unlit/MoveSpecificColor"
{
    Properties
    {
        _TargetColor ("Target Color", Color) = (0,0,0,1) // Target color to move
        _Speed ("Speed", Float) = 2.0
        _Amplitude ("Amplitude", Float) = 0.5
        _Threshold ("Threshold", Float) = 0.1 // Sensitivity for detecting target color
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
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _TargetColor;
            float _Speed;
            float _Amplitude;
            float _Threshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.color = v.color; // Pass vertex color to fragment shader

                // Compute the color difference between vertex color and target color
                float colorDiff = distance(v.color.rgb, _TargetColor.rgb);

                // Check if color is within the threshold
                float isMatching = step(colorDiff, _Threshold); // 1 if similar, 0 otherwise

                // Apply vertical movement only to matching colors
                float offset = sin(_Time.y * _Speed) * _Amplitude * isMatching;

                // Move the vertex on the Z-axis
                v.vertex.z += offset;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color; // Keep original vertex color
            }
            ENDCG
        }
    }
}
