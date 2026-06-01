Shader "Anomaly/CCTVEffect"
{
    Properties
    {
        _MainTex          ("Texture",            2D)             = "white" {}

        // --- 색수차 (Chromatic Aberration) ---
        _ChromaStrength   ("Chroma Strength",    Range(0, 0.03)) = 0.008

        // --- 비네트 (Vignette) ---
        _VignetteStrength ("Vignette Strength",  Range(0, 3))    = 1.5
        _VignetteSoft     ("Vignette Softness",  Range(0.01, 1)) = 0.3

        // --- 스캔라인 (Scanlines) ---
        _ScanStrength     ("Scanline Strength",  Range(0, 1))    = 0.18
        _ScanDensity      ("Scanline Density",   Range(100, 2000)) = 960

        // --- 필름 그레인 (Film Grain) ---
        _GrainStrength    ("Grain Strength",     Range(0, 0.5))  = 0.10
        _GrainSize        ("Grain Size",         Range(1, 10))   = 2.0

        // --- 글리치 (Glitch) ---
        _GlitchStrength   ("Glitch Strength",    Range(0, 1))    = 0.0
        _GlitchTime       ("Glitch Time",        Float)          = 0.0

        // --- 색조 틴트 (CCTV 녹색 틴트) ---
        _TintStrength     ("Tint Strength",      Range(0, 0.5))  = 0.08
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4    _MainTex_TexelSize;

            float _ChromaStrength;
            float _VignetteStrength;
            float _VignetteSoft;
            float _ScanStrength;
            float _ScanDensity;
            float _GrainStrength;
            float _GrainSize;
            float _GlitchStrength;
            float _GlitchTime;
            float _TintStrength;

            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 74.31);
                return frac(p.x * p.y);
            }

            float hash1(float n) { return frac(sin(n) * 43758.5453); }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                // ── 글리치: 수평 띠 어긋남 ────────────────────────
                if (_GlitchStrength > 0.001)
                {
                    float gt        = _GlitchTime;
                    float bandY     = floor(uv.y * 24.0);
                    float bandNoise = hash1(bandY + floor(gt * 8.0) * 3.7);
                    float trigger   = step(0.82 - _GlitchStrength * 0.6, bandNoise);
                    float shift     = (hash1(bandY * 17.3 + gt * 5.1) - 0.5)
                                      * _GlitchStrength * 0.06 * trigger;
                    uv.x = frac(uv.x + shift);

                    // RGB 채널 분리 강화
                    float cShift = _GlitchStrength * 0.015 * trigger;
                    float2 uvR = float2(frac(uv.x + cShift), uv.y);
                    float2 uvB = float2(frac(uv.x - cShift), uv.y);
                    float r0 = tex2D(_MainTex, uvR).r;
                    float g0 = tex2D(_MainTex, uv ).g;
                    float b0 = tex2D(_MainTex, uvB).b;
                    fixed4 glitchCol = fixed4(r0, g0, b0, 1.0);

                    // 색수차
                    float2 center = float2(0.5, 0.5);
                    float2 dir = normalize(uv - center);
                    float  dist = length(uv - center);
                    float  chromaShift = dist * (_ChromaStrength + _GlitchStrength * 0.01);
                    glitchCol.r = tex2D(_MainTex, uv + dir * chromaShift).r;
                    glitchCol.b = tex2D(_MainTex, uv - dir * chromaShift).b;

                    // 비네트
                    float2 vigUV  = uv - 0.5;
                    float  vigDot = dot(vigUV, vigUV) * _VignetteStrength;
                    glitchCol.rgb *= 1.0 - smoothstep(1.0 - _VignetteSoft, 1.0, vigDot);

                    // 스캔라인
                    float scan = sin(uv.y * _ScanDensity) * 0.5 + 0.5;
                    glitchCol.rgb *= 1.0 - _ScanStrength * (1.0 - scan);

                    // 그레인
                    float2 grainUV = floor(uv * _ScreenParams.xy / _GrainSize);
                    float  grain   = hash(grainUV + frac(_Time.y * 13.7));
                    glitchCol.rgb += (grain - 0.5) * (_GrainStrength + _GlitchStrength * 0.12);

                    // 녹색 틴트
                    glitchCol.rgb *= fixed3(1.0 - _TintStrength, 1.0 + _TintStrength, 1.0 - _TintStrength * 0.5);

                    return saturate(glitchCol);
                }

                // ── 일반 패스 ──────────────────────────────────────

                // 1. 색수차
                float2 center = float2(0.5, 0.5);
                float2 dir    = normalize(uv - center);
                float  dist   = length(uv - center);
                float  shift  = dist * _ChromaStrength;

                float r = tex2D(_MainTex, uv + dir * shift).r;
                float g = tex2D(_MainTex, uv             ).g;
                float b = tex2D(_MainTex, uv - dir * shift).b;
                fixed4 col = fixed4(r, g, b, 1.0);

                // 2. 비네트
                float2 vigUV  = uv - 0.5;
                float  vigDot = dot(vigUV, vigUV) * _VignetteStrength;
                col.rgb *= 1.0 - smoothstep(1.0 - _VignetteSoft, 1.0, vigDot);

                // 3. 스캔라인
                float scan = sin(uv.y * _ScanDensity) * 0.5 + 0.5;
                col.rgb   *= 1.0 - _ScanStrength * (1.0 - scan);

                // 4. 필름 그레인
                float2 grainUV = floor(uv * _ScreenParams.xy / _GrainSize);
                float  grain   = hash(grainUV + frac(_Time.y * 13.7));
                col.rgb += (grain - 0.5) * _GrainStrength;

                // 5. CCTV 녹색 틴트
                col.rgb *= fixed3(1.0 - _TintStrength, 1.0 + _TintStrength, 1.0 - _TintStrength * 0.5);

                return saturate(col);
            }
            ENDCG
        }
    }

    Fallback Off
}