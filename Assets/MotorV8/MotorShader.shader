Shader "Custom/MotorShader"
{
    Properties
    {
        _MainTex ("Base Color (Albedo)", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)

        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _RoughnessMap ("Roughness Map", 2D) = "white" {}

        _MotorPartMask ("Motor Part Mask", 2D) = "white" {}

       
        _RotationPivot ("Rotation Pivot", Vector) = (0,0,0)
        _RotationSpeed ("Rotation Speed", Float) = 1.0

        _PistonAxis("Piston Axis", Vector) = (0,0,0)

        _PistonBlueSpeed("Piston Blue Speed",Float) = 1.0
        _PistonRedSpeed("Piston Red Speed",Float) = 1.0
        _PistonBluePhase("Piston Blue Phase",Float) = 0.0
        _PistonRedPhase("Piston Red Phase",Float) = 0.0

        _ShakeFreq("Shake Frequency",Float) = 50
        _ShakeAmplitude("Shake Amplitude",Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _Color;

        float _RotationSpeed;
        float4 _RotationPivot;
        
        float4  _PistonAxis;

        float _PistonBlueSpeed;
        float _PistonRedSpeed;
        float _PistonBluePhase;
        float _PistonRedPhase;

        float _ShakeFreq;
        float _ShakeAmplitude;

        sampler2D _BumpMap;
        sampler2D _MetallicMap;
        sampler2D _RoughnessMap;
        sampler2D _MotorPartMask;
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_MetallicMap;
            float2 uv_RoughnessMap;
            float2 uv_MotorPartMask;
        };

        bool IsColor(float3 a, float3 b, float threshold)
        {
            return distance(a, b) < threshold;
        }

        void vert(inout appdata_full v)
    {
        float4 maskColor = tex2Dlod(_MotorPartMask, float4(v.texcoord.xy, 0, 0));
        float3 col = maskColor.rgb;

        float3 offset = float3(0, 0, 0);
        float t = _Time.y * 3.0;

        float3 axisOrigin = _PistonAxis.xyz;
        float3 axisDir = float3(1, 0, 0); // Assuming X axis for pistons

        float3 vertexToAxis = v.vertex.xyz - axisOrigin;
        float projectionLength = dot(vertexToAxis, axisDir);
        float3 closestPointOnAxis = axisOrigin + axisDir * projectionLength;
        float3 pistonDir = v.vertex.xyz - closestPointOnAxis;
        float3 dir = normalize(pistonDir);

        // Shake applied to all parts
        float shakeFreq = _ShakeFreq;
        float shakeAmp = _ShakeAmplitude;
        float shakeValue = sin(_Time.y * shakeFreq + dot(v.vertex.xyz, float3(3.1, 2.7, 1.5))) * shakeAmp + shakeAmp*sin(_Time.y * shakeFreq)*cos(_Time.y *2* shakeFreq);
        float3 shakeDir = normalize(cross(dir, float3(0.1, 0.9, 0.3))); // Arbitrary vector for shake direction
        float3 shakeOffset = shakeDir * shakeValue;

        if (IsColor(col, float3(1,0,0), 0.1)) // Red
        {
            float time = _Time.y * _PistonRedSpeed + _PistonRedPhase;
            float f = 0.5 * sin(time) - 0.5;
            offset = dir * f * 0.1;
        }
        else if (IsColor(col, float3(0,0,1), 0.1)) // Blue
        {
            float time = _Time.y * _PistonBlueSpeed + _PistonBluePhase;
            float f = 0.5 * sin(time) - 0.5;
            offset = dir * f * 0.1;
        }
        else if (IsColor(col, float3(0,0,0), 0.1)) // Black
        {
            float angle = _Time.y * _RotationSpeed;
            float s = sin(angle);
            float c = cos(angle);

            float3 pivot = _RotationPivot;
            float3 p = v.vertex.xyz - pivot;

            float y = p.y * c - p.z * s;
            float z = p.y * s + p.z * c;

            v.vertex.y = y + pivot.y;
            v.vertex.z = z + pivot.z;
        }

        // Apply movement offset (if any), then shake
        v.vertex.xyz += offset + shakeOffset;
    }

        

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float4 baseCol = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = baseCol.rgb;

            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            float metallic = tex2D(_MetallicMap, IN.uv_MetallicMap).r;
            float roughness = tex2D(_RoughnessMap, IN.uv_RoughnessMap).r;

            o.Metallic = metallic;
            o.Smoothness = 1.0 - roughness;
            o.Occlusion = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
