Shader "Custom/XRayThroughWalls"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)  // ✅ Normal visible color
        _XRayColor ("X-Ray Color", Color) = (0, 0.5, 1, 1)  // ✅ Blue X-Ray effect
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType"="Opaque" }
        Pass
        {
            Name "RegularRendering"
            Tags { "LightMode"="ForwardBase" }
            ZWrite On
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * _MainColor; // ✅ Uses texture and main color
            }
            ENDCG
        }

        Pass
        {
            Name "XRayRendering"
            ZWrite Off
            ZTest Greater // ✅ Only render where the object is behind something
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _XRayColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _XRayColor; // ✅ Only glows when hidden
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
