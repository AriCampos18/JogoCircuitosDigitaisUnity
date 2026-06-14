Shader "Custom/InwardShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        // Renderiza apenas as faces internas da esfera
        Cull Front

        CGPROGRAM
        #pragma surface surf Standard vertex:vert

        // Inverte as normais para a iluminação funcionar corretamente
        void vert(inout appdata_full v)
        {
            v.normal.xyz = v.normal * -1;
        }

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
        }

        ENDCG
    }

    FallBack "Diffuse"
}