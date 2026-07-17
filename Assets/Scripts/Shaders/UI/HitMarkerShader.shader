Shader "Custom/HitMarkerShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)   // driven by C# later: white/blue/red
        _Gap ("Gap", Range(0,0.5)) = 0.15       // inner endpoint distance from center (hole size)
        _Length ("Length", Range(0,0.7)) = 0.4  // outer endpoint distance from center (arm reach)
        _Thickness ("Thickness", Range(0,0.1)) = 0.02
        [Toggle] _WeakSpot("Weak Spot",float)=0
    }
    SubShader
    {
        // Same UI setup as the crosshair: transparent, no depth write, drawn both sides.
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            fixed4 _Color;
            float _Gap;
            float _Length;
            float _Thickness;
            float _WeakSpot;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;   // UI tint — lets the CanvasRenderer color pass through
                return o;
            }


            float sdf_segment(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;                              // A -> P
                float2 ba = b - a;                              // A -> B 
                float t = saturate(dot(pa, ba) / dot(ba, ba));  // fraction along, CLAMPED to segment
                float2 closest = a + t * ba;                    // walk t of the way from A to B
                return length(p - closest);                     // gap to that closest point
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Center the coords so (0,0) is the middle — same as the crosshair's i.uv - 0.5
                float2 uv = i.uv - 0.5;

                // Four diagonal lines built from _Gap and _Length by sign-flipping.
                // Each: inner point (near center, at _Gap) -> outer point (near corner, at _Length).
                float d = 1.0;  // start "far from everything"; min() will pull it down
                d = min(d, sdf_segment(uv, float2(-_Gap,-_Gap), float2(-_Length,-_Length)));
                d = min(d, sdf_segment(uv, float2( _Gap,-_Gap), float2( _Length,-_Length)));
                d = min(d, sdf_segment(uv, float2(-_Gap, _Gap), float2(-_Length, _Length)));
                d = min(d, sdf_segment(uv, float2( _Gap, _Gap), float2( _Length, _Length)));

                float wsDist = 1.0;
                wsDist = min(wsDist, sdf_segment(uv, float2(0, _Gap), float2(0, _Length)));   // up
                wsDist = min(wsDist, sdf_segment(uv, float2(0,-_Gap), float2(0,-_Length)));   // down
                wsDist = min(wsDist, sdf_segment(uv, float2(_Gap, 0), float2(_Length, 0)));   // right
                wsDist = min(wsDist, sdf_segment(uv, float2(-_Gap,0), float2(-_Length,0)));   // left

                // Gate: only fold the + in when _WeakSpot is on.
                // lerp keeps wsDist "far away" (1.0) when _WeakSpot=0, so it never draws.
                float gatedWs = lerp(1.0, wsDist, _WeakSpot);
                d = min(d, gatedWs);
                float fw = fwidth(d);
                float alpha = 1.0 - smoothstep(_Thickness - fw, _Thickness + fw,d);

                fixed4 col = _Color * i.color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}