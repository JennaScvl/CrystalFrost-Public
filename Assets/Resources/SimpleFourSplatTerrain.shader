Shader "CFEngine/SimpleFourSplatTerrain"
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}
        _Splat2 ("Texture 2", 2D) = "white" {}
        _Splat3 ("Texture 3", 2D) = "white" {}
        _Splat4 ("Texture 4", 2D) = "white" {}
        _SplatMap ("Splat Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_SplatMap;
        };

        sampler2D _MainTex;
        sampler2D _Splat2;
        sampler2D _Splat3;
        sampler2D _Splat4;
        sampler2D _SplatMap;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 splatWeights = tex2D(_SplatMap, IN.uv_SplatMap);
            half4 color1 = tex2D(_MainTex, IN.uv_MainTex);
            half4 color2 = tex2D(_Splat2, IN.uv_MainTex);
            half4 color3 = tex2D(_Splat3, IN.uv_MainTex);
            half4 color4 = tex2D(_Splat4, IN.uv_MainTex);
                
            half4 finalColor;
            finalColor = color1 * splatWeights.r + color2 * splatWeights.g + color3 * splatWeights.b + color4 * splatWeights.a;

            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
}
