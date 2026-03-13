Shader "Custom/RetroVHS"
{
    Properties
    {
        // Scanlines
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 1.0)) = 0.3
        _ScanlineCount ("Scanline Count", Range(50, 1500)) = 400
        _ScanlineSpeed ("Scanline Scroll Speed", Range(0.0, 10.0)) = 0.5

        // Noise / Static
        _NoiseIntensity ("Noise Intensity", Range(0.0, 1.0)) = 0.1
        _NoiseSpeed ("Noise Animation Speed", Range(0.0, 50.0)) = 15.0

        // Horizontal Glitch
        _GlitchIntensity ("Glitch Intensity", Range(0.0, 0.1)) = 0.01
        _GlitchSpeed ("Glitch Speed", Range(0.0, 20.0)) = 5.0
        _GlitchBlockSize ("Glitch Block Size", Range(0.001, 0.2)) = 0.05

        // Chromatic Aberration (VHS style - horizontal offset)
        _RGBOffsetIntensity ("RGB Offset Intensity", Range(0.0, 0.02)) = 0.005

        // Color Bleed / Phosphor
        _ColorBleedAmount ("Color Bleed Amount", Range(0.0, 10.0)) = 2.0
        _PhosphorBleed ("Phosphor Bleed", Range(0.0, 1.0)) = 0.3

        // Flicker
        _FlickerIntensity ("Flicker Intensity", Range(0.0, 0.2)) = 0.03
        _FlickerSpeed ("Flicker Speed", Range(0.0, 50.0)) = 15.0

        // Color Grading
        _Saturation ("Saturation", Range(0.0, 2.0)) = 0.85
        _ColorTint ("Color Tint", Color) = (0.9, 1.0, 0.95, 1.0)
        _Brightness ("Brightness", Range(0.5, 2.0)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2.0)) = 1.1

        // Interlacing
        _InterlaceIntensity ("Interlace Intensity", Range(0.0, 1.0)) = 0.1
        _InterlaceSpeed ("Interlace Speed", Range(0.0, 10.0)) = 1.0

        // Jitter (vertical hold wobble)
        _JitterIntensity ("Jitter Intensity", Range(0.0, 0.02)) = 0.002
        _JitterSpeed ("Jitter Speed", Range(0.0, 30.0)) = 8.0

        // Tape Wrinkle (horizontal distortion band that moves vertically)
        _WrinkleIntensity ("Tape Wrinkle Intensity", Range(0.0, 0.05)) = 0.01
        _WrinkleSpeed ("Tape Wrinkle Speed", Range(0.0, 5.0)) = 0.8
        _WrinkleSize ("Tape Wrinkle Size", Range(0.01, 0.3)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "RetroVHSPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // --- Parameters ---
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineSpeed;

            float _NoiseIntensity;
            float _NoiseSpeed;

            float _GlitchIntensity;
            float _GlitchSpeed;
            float _GlitchBlockSize;

            float _RGBOffsetIntensity;

            float _ColorBleedAmount;
            float _PhosphorBleed;

            float _FlickerIntensity;
            float _FlickerSpeed;

            float _Saturation;
            float4 _ColorTint;
            float _Brightness;
            float _Contrast;

            float _InterlaceIntensity;
            float _InterlaceSpeed;

            float _JitterIntensity;
            float _JitterSpeed;

            float _WrinkleIntensity;
            float _WrinkleSpeed;
            float _WrinkleSize;

            // -----------------------------------------------------------
            // Hash-based pseudo-random number generators
            // -----------------------------------------------------------
            float Hash(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }

            float Hash2D(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // -----------------------------------------------------------
            // Scanline effect — darkens alternating horizontal lines
            // -----------------------------------------------------------
            float Scanline(float2 uv)
            {
                float scanVal = sin((uv.y + _Time.y * _ScanlineSpeed * 0.01) * _ScanlineCount * 3.14159);
                return 1.0 - _ScanlineIntensity * (scanVal * 0.5 + 0.5);
            }

            // -----------------------------------------------------------
            // TV static noise overlay
            // -----------------------------------------------------------
            float StaticNoise(float2 uv)
            {
                float t = _Time.y * _NoiseSpeed;
                return Hash2D(uv * 500.0 + float2(t, t * 0.7));
            }

            // -----------------------------------------------------------
            // Horizontal glitch — displaces horizontal bands randomly
            // -----------------------------------------------------------
            float GlitchOffset(float y)
            {
                float t = floor(_Time.y * _GlitchSpeed);
                float blockY = floor(y / _GlitchBlockSize);
                float trigger = step(0.92, Hash(blockY + t * 7.0));
                float offset = (Hash(blockY * 3.0 + t * 13.0) - 0.5) * 2.0;
                return offset * _GlitchIntensity * trigger;
            }

            // -----------------------------------------------------------
            // VHS tape wrinkle — horizontal distortion band moving down
            // -----------------------------------------------------------
            float TapeWrinkle(float y)
            {
                float bandPos = frac(_Time.y * _WrinkleSpeed);
                float dist = abs(y - bandPos);
                float band = exp(-dist * dist / (_WrinkleSize * _WrinkleSize));
                float displacement = sin(y * 200.0 + _Time.y * 30.0) * band;
                return displacement * _WrinkleIntensity;
            }

            // -----------------------------------------------------------
            // Color bleed — CRT phosphor bleeding (fixed 7-tap horizontal blur)
            // Uses a fixed loop to avoid dynamic loop bound compilation errors
            // -----------------------------------------------------------
            half3 ColorBleed(float2 uv, float2 texelSize)
            {
                // Fixed 7-tap gaussian-weighted horizontal blur
                // Offsets: -3, -2, -1, 0, +1, +2, +3
                static const int TAPS = 7;
                static const float offsets[TAPS] = { -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0 };
                static const float weights[TAPS] = { 0.006, 0.061, 0.242, 0.383, 0.242, 0.061, 0.006 };

                half3 sum = half3(0, 0, 0);
                float bleedScale = texelSize.x * _ColorBleedAmount;

                [unroll]
                for (int i = 0; i < TAPS; i++)
                {
                    float2 sampleUV = float2(uv.x + offsets[i] * bleedScale, uv.y);
                    sum += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, sampleUV).rgb * weights[i];
                }

                return sum;
            }

            // -----------------------------------------------------------
            // Fragment shader — composites all effects
            // -----------------------------------------------------------
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Compute texel size from _ScreenParams (always available in URP)
                // _ScreenParams.xy = (width, height)
                float2 texelSize = 1.0 / _ScreenParams.xy;

                // --- Vertical jitter (VHS tracking wobble) ---
                uv.x += sin(uv.y * 100.0 + _Time.y * _JitterSpeed) * _JitterIntensity;

                // --- Tape wrinkle distortion ---
                uv.x += TapeWrinkle(uv.y);

                // --- Horizontal glitch displacement ---
                uv.x += GlitchOffset(uv.y);

                // --- VHS chromatic aberration (horizontal RGB split) ---
                half3 col;
                float rgbOff = _RGBOffsetIntensity;
                col.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, float2(uv.x - rgbOff, uv.y)).r;
                col.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                col.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, float2(uv.x + rgbOff, uv.y)).b;

                // --- Phosphor color bleed ---
                half3 bleed = ColorBleed(uv, texelSize);
                col = lerp(col, bleed, _PhosphorBleed);

                // --- Scanlines ---
                col *= Scanline(uv);

                // --- Interlacing (darken every other line, alternating each frame) ---
                float screenY = uv.y * _ScreenParams.y;
                float interlace = fmod(floor(screenY) + floor(_Time.y * _InterlaceSpeed * 30.0), 2.0);
                col *= 1.0 - _InterlaceIntensity * interlace;

                // --- Static noise overlay ---
                float noise = StaticNoise(uv);
                col = lerp(col, half3(noise, noise, noise), _NoiseIntensity);

                // --- Screen flicker ---
                float flicker = 1.0 + (Hash(floor(_Time.y * _FlickerSpeed)) - 0.5) * _FlickerIntensity;
                col *= flicker;

                // --- Color grading ---
                // Saturation
                float luma = dot(col, half3(0.299, 0.587, 0.114));
                col = lerp(half3(luma, luma, luma), col, _Saturation);

                // Tint
                col *= _ColorTint.rgb;

                // Brightness and contrast
                col = (col - 0.5) * _Contrast + 0.5;
                col *= _Brightness;

                return half4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
