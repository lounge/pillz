Shader "Sprites/SpriteGlow2D"
{
    Properties
    {
        // ⬇⬇ Make SpriteRenderer bind the per-renderer sprite texture
        [PerRendererData] [NoScaleOffset] [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}

        [MainColor] _Color ("Base Color (RGBA)", Color) = (1,1,1,1)
        _GlowColor ("Glow Color (HDR)", Color) = (1,1,1,1)
        _GlowStrength ("Glow Strength", Float) = 2.0
        _PulseSpeed ("Pulse Speed (0 = off)", Float) = 0.0
        _PulseMin ("Pulse Min", Float) = 0.6
        _PulseMax ("Pulse Max", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SpriteUnlit2D"
            Tags
            {
                "LightMode"="Universal2D"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float _GlowStrength, _PulseSpeed, _PulseMin, _PulseMax;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS:POSITION;
                float2 uv:TEXCOORD0;
                float4 color:COLOR;
            };

            struct Varyings
            {
                float4 positionHCS:SV_POSITION;
                float2 uv:TEXCOORD0;
                float4 color:COLOR;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;            // no TRANSFORM_TEX
                o.color = v.color; // SpriteRenderer tint
                return o;
            }

            half4 frag(Varyings i):SV_Target
            {
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Base sprite color uses sprite alpha for silhouette
                float4 baseCol = float4(_Color.rgb * i.color.rgb, _Color.a * i.color.a * tex.a);

                // Pulse
                float p01 = (_PulseSpeed <= 0) ? 1.0 : 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);
                float pulse = lerp(_PulseMin, _PulseMax, p01);

                float3 emissive = _GlowColor.rgb * (_GlowStrength * pulse);
                return half4(baseCol.rgb + emissive, baseCol.a);
            }
            ENDHLSL
        }
    }
}