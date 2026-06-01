Shader "Custom/WhiteBackgroundRemove"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Threshold ("White Threshold", Range(0.5, 1.0)) = 0.85
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float4 color : COLOR; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Threshold;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                // 밝기 계산 (grayscale)
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114));
                // 흰 배경(밝은 회색~흰색)이면 투명 처리
                float whiteMask = smoothstep(_Threshold - 0.05, _Threshold, brightness);
                col.a *= (1.0 - whiteMask);
                return col;
            }
            ENDCG
        }
    }
}