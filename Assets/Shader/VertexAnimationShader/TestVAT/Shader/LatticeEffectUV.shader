Shader "Unlit/LatticeEffectUV"
{
    Properties
    {
        _TargetColor ("Expand Target Color", Color) = (1,0,0,1)  // Target color to increase size
        _ShrinkTargetColor ("Shrink Target Color", Color) = (0,0,1,1) // Target color to reduce size
        _ColorThreshold ("Color Matching Threshold", Range(0,1)) = 0.2 // Sensitivity for both effects
        _Strength ("Deformation Strength", Range(0,50)) = 1.0  // Intensity of effect (both expand and shrink)
        _ScaleFactor ("Scale Factor", Range(0,50)) = 1.0  // Overall scaling effect
        _AxisInfluence ("Axis Influence (X,Y,Z)", Vector) = (1,1,1,0) // Custom scaling per axis
        _MainTex ("Albedo (RGB)", 2D) = "white" {} // Texture input
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR; // Vertex color input
        };

        sampler2D _MainTex;
        float3 _TargetColor;        // Expand color
        float3 _ShrinkTargetColor;  // Shrink color
        float _ColorThreshold;
        float _Strength;
        float _ScaleFactor;
        float3 _AxisInfluence; // Custom axis influence

        void vert(inout appdata_full v)
        {
            // Get vertex color
            float3 color = v.color.rgb;

            // Read displacement direction from TEXCOORD2
            float3 displacementDir = v.texcoord .xyz; 

            // Compute similarity to _TargetColor (expand effect)
            float expandMatch = 1.0 - distance(color, _TargetColor);
            expandMatch = saturate(expandMatch / _ColorThreshold); // Normalize to 0-1
            float expandInfluence = smoothstep(0.0, 1.0, expandMatch);

            // Compute similarity to _ShrinkTargetColor (shrink effect)
            float shrinkMatch = 1.0 - distance(color, _ShrinkTargetColor);
            shrinkMatch = saturate(shrinkMatch / _ColorThreshold); // Normalize to 0-1
            float shrinkInfluence = smoothstep(0.0, 1.0, shrinkMatch);

            // Combine influences: expand increases size, shrink reduces size
            float3 expandDeformation = displacementDir * expandInfluence * _ScaleFactor * _Strength;
            float3 shrinkDeformation = displacementDir * shrinkInfluence * _ScaleFactor * _Strength * -1.0; // Negative for shrinking

            // Net deformation: expand + shrink
            float3 deformation = expandDeformation + shrinkDeformation;

            // Apply the scaling effect to the vertex position
            v.vertex.xyz += deformation; // Move vertex along the stored direction
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {   
            o.Albedo = IN.color.rgb; // Keep vertex color as surface color
            //o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb; // Use original mesh texture
        }
        ENDCG
    }
}
                                         
