Shader "Custom/GlowingRotatingRing"
{
    Properties
    {
        _MainTex        ("Texture (optional)", 2D) = "white" {}
        _TintColor      ("Tint Color", Color) = (1,1,1,1)
        _GlowIntensity  ("Glow Intensity", Float) = 3

        _Radius         ("Ring Radius (0..0.7)", Range(0.0, 0.7)) = 0.35
        _Thickness      ("Ring Thickness", Range(0.001, 0.5)) = 0.08
        _Softness       ("Edge Softness", Range(0.0, 0.2)) = 0.04

        _RotationSpeed  ("Rotation Speed (rad/s)", Float) = 2.0
        _Spokes         ("Spokes (0 = none)", Range(0, 24)) = 4
        _SpokeContrast  ("Spoke Contrast", Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend One One
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _TintColor;
            float  _GlowIntensity;

            float  _Radius;
            float  _Thickness;
            float  _Softness;

            float  _RotationSpeed;
            int    _Spokes;
            float  _SpokeContrast;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Soft ring mask centered at (0.5, 0.5)
            float ringMask(float2 uv, float radius, float thickness, float softness)
            {
                float2 d = uv - 0.5;
                float r = length(d);

                // distance from ring centerline
                float dr = abs(r - radius);

                // inner/outer falloff
                float halfT = max(1e-5, thickness * 0.5);
                float edge = smoothstep(halfT + softness, halfT, dr);

                return saturate(edge);
            }

            // Angular modulation to make rotation visible (spokes or a bright sweep)
            float angularMod(float2 uv, int spokes, float t, float contrast)
            {
                if (spokes <= 0) return 1.0;

                float2 d = uv - 0.5;
                // angle in radians (-pi..pi)
                float ang = atan2(d.y, d.x);
                ang += t; // rotate over time

                // Spokes via cosine; normalize 0..1
                float m = 0.5 + 0.5 * cos(ang * (float)spokes);
                // Increase contrast (pow curve)
                m = pow(m, max(1e-4, 1.0 / max(1e-4, contrast)));
                return m;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Base texture (alpha as mask if you want to stencil)
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Ring mask (soft)
                float ring = ringMask(i.uv, _Radius, _Thickness, _Softness);

                // Time for rotation
                float t = _Time.y * _RotationSpeed;

                // Angular modulation (spokes) so rotation is visible
                float mod = angularMod(i.uv, _Spokes, t, _SpokeContrast);

                // Final mask
                float mask = ring * mod;

                // Color & glow (additive)
                fixed4 col = _TintColor * tex;
                col.rgb *= _GlowIntensity * mask;
                col.a   = mask * _TintColor.a * tex.a;

                return col;
            }
            ENDCG
        }
    }
}
