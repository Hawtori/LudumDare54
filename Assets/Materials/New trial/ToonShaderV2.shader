Shader "Unlit/ToonShaderV2"
{
    Properties
    {
        _Albedo("Albedo", Color) = (1, 1, 1, 1)
        _Shades("Shades", Rnage(1, 20)) = 3
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
            };

            float4 _Albedo;
            float _Shades;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float cosAngle = dot(normalize(i.worldNormal), normalize(_WorldSpaceLightPos0.xyz));

                cosAngle = max(cosAngle, 0.0);

                cosAngle = floor(cosAngle * _Shades) / _Shades;
                
                return _Albedo * cosAngle;
            }
            ENDCG
        }
    }
}
