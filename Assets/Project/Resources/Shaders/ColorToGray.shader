Shader "Custom/URP/2D/Unlit/ColorToGray"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GrayAmount  ("Gray Amount", Range(0,1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"        = "Transparent"
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "CanUseSpriteAtlas" = "True"
        }
        LOD 100

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float  _GrayAmount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionH : SV_POSITION;
                float2 uv        : TEXCOORD0;
                float4 color     : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionH = TransformObjectToHClip(IN.positionOS);
                OUT.uv        = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color     = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                col.rgb = lerp(col.rgb, dot(col.rgb, float3(0.3, 0.59, 0.11)), _GrayAmount);
                return col;
            }
            ENDHLSL
        }
    }
}
