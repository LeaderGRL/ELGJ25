Shader "Custom/BarrelDistortion"
{
    Properties
    {
        // Properties exposed in material inspector for default values
        // At runtime, BarrelDistortionController sets these via Material.SetFloat
        _Distortion ("Distortion Strength", Range(-1.0, 1.0)) = 0.3
        _CubicDistortion ("Cubic Distortion", Range(-1.0, 1.0)) = 0.1
        _Zoom ("Zoom (compensate edges)", Range(0.5, 2.0)) = 1.0
        _ChromaticAberration ("Chromatic Aberration", Range(0.0, 0.05)) = 0.005
        _VignetteStrength ("Vignette Strength", Range(0.0, 2.0)) = 0.5
        _VignetteRadius ("Vignette Radius", Range(0.0, 2.0)) = 0.8
        _VignetteSoftness ("Vignette Softness", Range(0.01, 1.0)) = 0.5
        _ScreenCurvature ("Screen Curvature", Range(0.0, 1.0)) = 0.0
        _CornerDarkness ("Corner Darkness", Range(0.0, 1.0)) = 0.3
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
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
            Name "BarrelDistortionPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            // Core URP + Blit includes
            // Blit.hlsl provides: Vert vertex shader, Varyings struct (.texcoord),
            // _BlitTexture and sampler_LinearClamp
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Distortion parameters — set by BarrelDistortionController at runtime
            float _Distortion;
            float _CubicDistortion;
            float _Zoom;
            float _ChromaticAberration;
            float _VignetteStrength;
            float _VignetteRadius;
            float _VignetteSoftness;
            float _ScreenCurvature;
            float _CornerDarkness;
            float4 _BackgroundColor;

            // ---------------------------------------------------------------
            // Barrel distortion using Brown-Conrady model (quadratic + cubic)
            // ---------------------------------------------------------------
            float2 DistortUV(float2 uv, float strength, float cubic)
            {
                float2 centered = uv - 0.5;
                float r2 = dot(centered, centered);
                float r4 = r2 * r2;
                float factor = 1.0 + strength * r2 + cubic * r4;
                return centered * factor / _Zoom + 0.5;
            }

            // Check if UV is within [0,1] bounds
            float IsInBounds(float2 uv)
            {
                float2 s = step(0.0, uv) * step(uv, 1.0);
                return s.x * s.y;
            }

            // CRT-style screen curvature
            float2 ApplyCurvature(float2 uv, float curvature)
            {
                float2 c = uv * 2.0 - 1.0;
                float2 offset = c.yx * c.yx * curvature;
                c += c * offset;
                return c * 0.5 + 0.5;
            }

            // Vignette darkening factor
            float ComputeVignette(float2 uv)
            {
                float2 centered = uv - 0.5;
                float dist = length(centered);
                float v = smoothstep(_VignetteRadius, _VignetteRadius - _VignetteSoftness, dist);
                return lerp(1.0, v, _VignetteStrength);
            }

            // Sample the source texture (_BlitTexture is set by the Blitter/AddBlitPass API)
            half3 SampleSource(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Apply optional CRT curvature
                if (_ScreenCurvature > 0.001)
                {
                    uv = ApplyCurvature(uv, _ScreenCurvature);
                }

                // Compute distorted UVs
                float2 distortedUV = DistortUV(uv, _Distortion, _CubicDistortion);

                half4 col = half4(0, 0, 0, 1);

                if (_ChromaticAberration > 0.0001)
                {
                    // Chromatic aberration: offset R and B distortion slightly
                    float2 uvR = DistortUV(uv, _Distortion + _ChromaticAberration, _CubicDistortion);
                    float2 uvG = distortedUV;
                    float2 uvB = DistortUV(uv, _Distortion - _ChromaticAberration, _CubicDistortion);

                    float bR = IsInBounds(uvR);
                    float bG = IsInBounds(uvG);
                    float bB = IsInBounds(uvB);

                    col.r = lerp(_BackgroundColor.r, SampleSource(uvR).r, bR);
                    col.g = lerp(_BackgroundColor.g, SampleSource(uvG).g, bG);
                    col.b = lerp(_BackgroundColor.b, SampleSource(uvB).b, bB);
                }
                else
                {
                    float bounds = IsInBounds(distortedUV);
                    half3 sampled = SampleSource(distortedUV);
                    col.rgb = lerp(_BackgroundColor.rgb, sampled, bounds);
                }

                // Vignette
                col.rgb *= ComputeVignette(uv);

                // Corner darkness
                if (_CornerDarkness > 0.001)
                {
                    float2 c = uv * 2.0 - 1.0;
                    float cornerFade = 1.0 - smoothstep(1.0, 1.8, length(c)) * _CornerDarkness;
                    col.rgb *= cornerFade;
                }

                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
