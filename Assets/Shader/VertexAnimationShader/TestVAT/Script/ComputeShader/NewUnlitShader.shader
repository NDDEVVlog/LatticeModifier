Shader "Custom/FFDStandardShader_Frag"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MetallicMap ("Metallic (R)", 2D) = "white" {}
        _AOMap ("Ambient Occlusion (R)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}

        _Metallic ("Metallic", Range(0,1)) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _EmissionStrength ("Emission Strength", Range(0,5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityStandardBRDF.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT; // Needed for normal mapping
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBitangent : TEXCOORD4;
                UNITY_FOG_COORDS(5)
            };

            sampler2D _MainTex;
            sampler2D _MetallicMap;
            sampler2D _AOMap;
            sampler2D _NormalMap;
            sampler2D _EmissionMap;

            float _Metallic;
            float _Smoothness;
            float _EmissionStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // Calculate normal mapping vectors
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBitangent = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample Albedo
                // Sample Albedo
                fixed4 albedoColor = tex2D(_MainTex, i.uv);

                // Sample normal map
                float3 normalMap = UnpackNormal(tex2D(_NormalMap, i.uv));
                float3x3 TBN = float3x3(i.worldTangent, i.worldBitangent, i.worldNormal);
                float3 normal = normalize(mul(normalMap, TBN));

                // Sample metallic and AO maps
                float metallic = tex2D(_MetallicMap, i.uv).r * _Metallic;
                float ao = tex2D(_AOMap, i.uv).r;

                // Sample emission map
                float3 emission = tex2D(_EmissionMap, i.uv).rgb * _EmissionStrength;

                // Compute lighting (Blinn-Phong model)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = max(0, dot(normal, lightDir));

                // Lambertian diffuse shading
                float3 diffuse = albedoColor.rgb * NdotL;

                // Specular reflection (Blinn-Phong)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(viewDir + lightDir);
                float NdotH = max(0, dot(normal, halfDir));
                float specular = pow(NdotH, (1.0 - _Smoothness) * 100.0) * metallic;

                // Apply AO
                diffuse *= ao;

                // Final Color Output
                float3 finalColor = diffuse + specular + emission;

                // Apply fog
                fixed4 col = fixed4(finalColor, albedoColor.a);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
