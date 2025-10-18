Shader "Custom/CrosshairShader"
{
   Properties
   {
        _MainTex ("Texture", 2D) = "white" {}
        //Reticle Color
        _Color ("Color",Color) = (1,1,1,1) //White
        _OutlineColor ("Outline Color",Color) = (0,0,0,1) // Black
        //Reticle Size
        _InnerRadius ("Inner Radius", Range(0,1)) = 0.2
        _OuterRadius ("Outer Radius", Range(0,1)) = 0.4

        _Thickness ("Thickness", Range(0,0.1)) = 0.01
        _DashCount ("Dash Count", Int) = 4
   }
SubShader
    {
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
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _InnerRadius;
            float _OuterRadius;
            float _Thickness;
            float _DashCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float sdf_ring(float2 p, float r, float t)
            {
                return abs(length(p) - r) - t * 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                
                // Calcular distancias para los anillos principales
                float outerRing = sdf_ring(uv, _OuterRadius, _Thickness);
                float innerRing = sdf_ring(uv, _InnerRadius, _Thickness);

                // Calcular distancias para los bordes (outline) - anillos más gruesos
                float outerRingOutline = sdf_ring(uv, _OuterRadius, _Thickness + _OutlineWidth * 2.0);
                float innerRingOutline = sdf_ring(uv, _InnerRadius, _Thickness + _OutlineWidth * 2.0);

                // Crear máscara para los dashes
                float angle = atan2(uv.y, uv.x);
                float a = (angle + 3.14159) / 6.28318;
                float dashMask = step(0.5, frac(a * _DashCount));

                // Anti-aliasing
                float fw = fwidth(length(uv)) * 1.0;

                // Calcular alphas de los anillos principales
                float outerAlpha = 1.0 - smoothstep(-fw, fw, outerRing);
                float innerAlpha = 1.0 - smoothstep(-fw, fw, innerRing);
                innerAlpha *= dashMask;

                // Calcular alphas de los bordes
                float outerOutlineAlpha = 1.0 - smoothstep(-fw, fw, outerRingOutline);
                float innerOutlineAlpha = 1.0 - smoothstep(-fw, fw, innerRingOutline);
                innerOutlineAlpha *= dashMask;

                // Combinar los anillos principales
                float mainAlpha = max(outerAlpha, innerAlpha);
                
                // Combinar los bordes
                float outlineAlpha = max(outerOutlineAlpha, innerOutlineAlpha);
                
                // Restar los anillos principales del borde para que solo se vea alrededor
                outlineAlpha = outlineAlpha * (1.0 - mainAlpha);

                // Combinar colores: primero el borde (atrás), luego el principal (adelante)
                fixed4 outlineCol = _OutlineColor * i.color;
                outlineCol.a *= outlineAlpha;
                
                fixed4 mainCol = _Color * i.color;
                mainCol.a *= mainAlpha;

                // Blend: el color principal sobre el borde
                fixed4 finalCol = lerp(outlineCol, mainCol, mainAlpha);
                finalCol.a = max(outlineAlpha, mainAlpha);

                return finalCol;
            }
            ENDCG
        }
    }
}