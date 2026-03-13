using UnityEngine;

/// <summary>
/// Runtime controller for the RetroVHS CRT/VHS effect.
/// Updates material properties every frame for live tweaking.
/// Includes presets inspired by different retro aesthetics.
///
/// Attach to any GameObject (typically the camera).
/// Assign the SAME material used in RetroVHSFeature.
/// </summary>
[ExecuteInEditMode]
public class RetroVHSController : MonoBehaviour
{
    [Header("=== Material ===")]
    [Tooltip("Same material assigned to RetroVHSFeature")]
    public Material vhsMaterial;

    [Header("=== Scanlines ===")]
    [Range(0f, 1f)]   public float scanlineIntensity = 0.3f;
    [Range(50f, 1500f)] public float scanlineCount = 400f;
    [Range(0f, 10f)]  public float scanlineSpeed = 0.5f;

    [Header("=== Noise / Static ===")]
    [Range(0f, 1f)]   public float noiseIntensity = 0.1f;
    [Range(0f, 50f)]  public float noiseSpeed = 15f;

    [Header("=== Horizontal Glitch ===")]
    [Range(0f, 0.1f)] public float glitchIntensity = 0.01f;
    [Range(0f, 20f)]  public float glitchSpeed = 5f;
    [Range(0.001f, 0.2f)] public float glitchBlockSize = 0.05f;

    [Header("=== VHS RGB Offset ===")]
    [Range(0f, 0.02f)] public float rgbOffsetIntensity = 0.005f;

    [Header("=== Phosphor / Color Bleed ===")]
    [Range(0f, 10f)]  public float colorBleedAmount = 2f;
    [Range(0f, 1f)]   public float phosphorBleed = 0.3f;

    [Header("=== Flicker ===")]
    [Range(0f, 0.2f)] public float flickerIntensity = 0.03f;
    [Range(0f, 50f)]  public float flickerSpeed = 15f;

    [Header("=== Color Grading ===")]
    [Range(0f, 2f)]   public float saturation = 0.85f;
    public Color colorTint = new Color(0.9f, 1f, 0.95f, 1f);
    [Range(0.5f, 2f)] public float brightness = 1f;
    [Range(0.5f, 2f)] public float contrast = 1.1f;

    [Header("=== Interlacing ===")]
    [Range(0f, 1f)]   public float interlaceIntensity = 0.1f;
    [Range(0f, 10f)]  public float interlaceSpeed = 1f;

    [Header("=== VHS Jitter ===")]
    [Range(0f, 0.02f)] public float jitterIntensity = 0.002f;
    [Range(0f, 30f)]  public float jitterSpeed = 8f;

    [Header("=== Tape Wrinkle ===")]
    [Range(0f, 0.05f)] public float wrinkleIntensity = 0.01f;
    [Range(0f, 5f)]   public float wrinkleSpeed = 0.8f;
    [Range(0.01f, 0.3f)] public float wrinkleSize = 0.05f;

    [Header("=== Presets ===")]
    public VHSPreset preset = VHSPreset.None;

    public enum VHSPreset
    {
        None,
        Warakami1984,
        CleanCRT,
        DirtyVHS,
        GlitchHeavy,
        Vaporwave,
        SecurityCam
    }

    private void OnEnable()
    {
        if (preset != VHSPreset.None)
            ApplyPreset(preset);
    }

    private void Update()
    {
        PushToMaterial();
    }

    /// <summary>
    /// Sends all parameter values to the shader material each frame.
    /// </summary>
    private void PushToMaterial()
    {
        if (vhsMaterial == null) return;

        vhsMaterial.SetFloat("_ScanlineIntensity", scanlineIntensity);
        vhsMaterial.SetFloat("_ScanlineCount", scanlineCount);
        vhsMaterial.SetFloat("_ScanlineSpeed", scanlineSpeed);

        vhsMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        vhsMaterial.SetFloat("_NoiseSpeed", noiseSpeed);

        vhsMaterial.SetFloat("_GlitchIntensity", glitchIntensity);
        vhsMaterial.SetFloat("_GlitchSpeed", glitchSpeed);
        vhsMaterial.SetFloat("_GlitchBlockSize", glitchBlockSize);

        vhsMaterial.SetFloat("_RGBOffsetIntensity", rgbOffsetIntensity);

        vhsMaterial.SetFloat("_ColorBleedAmount", colorBleedAmount);
        vhsMaterial.SetFloat("_PhosphorBleed", phosphorBleed);

        vhsMaterial.SetFloat("_FlickerIntensity", flickerIntensity);
        vhsMaterial.SetFloat("_FlickerSpeed", flickerSpeed);

        vhsMaterial.SetFloat("_Saturation", saturation);
        vhsMaterial.SetColor("_ColorTint", colorTint);
        vhsMaterial.SetFloat("_Brightness", brightness);
        vhsMaterial.SetFloat("_Contrast", contrast);

        vhsMaterial.SetFloat("_InterlaceIntensity", interlaceIntensity);
        vhsMaterial.SetFloat("_InterlaceSpeed", interlaceSpeed);

        vhsMaterial.SetFloat("_JitterIntensity", jitterIntensity);
        vhsMaterial.SetFloat("_JitterSpeed", jitterSpeed);

        vhsMaterial.SetFloat("_WrinkleIntensity", wrinkleIntensity);
        vhsMaterial.SetFloat("_WrinkleSpeed", wrinkleSpeed);
        vhsMaterial.SetFloat("_WrinkleSize", wrinkleSize);
    }

    /// <summary>
    /// Applies a named preset. Call from code or set in Inspector.
    /// </summary>
    public void ApplyPreset(VHSPreset target)
    {
        switch (target)
        {
            case VHSPreset.Warakami1984:
                // The Warakami vaporwave/retro style from the reference
                scanlineIntensity = 0.25f;
                scanlineCount = 500f;
                scanlineSpeed = 0.3f;
                noiseIntensity = 0.08f;
                noiseSpeed = 15f;
                glitchIntensity = 0.008f;
                glitchSpeed = 4f;
                glitchBlockSize = 0.04f;
                rgbOffsetIntensity = 0.004f;
                colorBleedAmount = 3f;
                phosphorBleed = 0.35f;
                flickerIntensity = 0.02f;
                flickerSpeed = 12f;
                saturation = 0.9f;
                colorTint = new Color(0.85f, 1f, 0.92f, 1f); // Slight cyan/green tint
                brightness = 1.05f;
                contrast = 1.15f;
                interlaceIntensity = 0.08f;
                interlaceSpeed = 1f;
                jitterIntensity = 0.001f;
                jitterSpeed = 6f;
                wrinkleIntensity = 0.005f;
                wrinkleSpeed = 0.6f;
                wrinkleSize = 0.04f;
                break;

            case VHSPreset.CleanCRT:
                // Subtle CRT with just scanlines and slight vignette feel
                scanlineIntensity = 0.15f;
                scanlineCount = 600f;
                scanlineSpeed = 0f;
                noiseIntensity = 0.02f;
                noiseSpeed = 10f;
                glitchIntensity = 0f;
                glitchSpeed = 0f;
                glitchBlockSize = 0.05f;
                rgbOffsetIntensity = 0.002f;
                colorBleedAmount = 1f;
                phosphorBleed = 0.15f;
                flickerIntensity = 0.01f;
                flickerSpeed = 10f;
                saturation = 0.95f;
                colorTint = new Color(1f, 1f, 1f, 1f);
                brightness = 1f;
                contrast = 1.05f;
                interlaceIntensity = 0.05f;
                interlaceSpeed = 1f;
                jitterIntensity = 0f;
                jitterSpeed = 0f;
                wrinkleIntensity = 0f;
                wrinkleSpeed = 0f;
                wrinkleSize = 0.05f;
                break;

            case VHSPreset.DirtyVHS:
                // Heavy tape degradation — worn out cassette feel
                scanlineIntensity = 0.35f;
                scanlineCount = 350f;
                scanlineSpeed = 0.8f;
                noiseIntensity = 0.2f;
                noiseSpeed = 20f;
                glitchIntensity = 0.025f;
                glitchSpeed = 6f;
                glitchBlockSize = 0.08f;
                rgbOffsetIntensity = 0.01f;
                colorBleedAmount = 5f;
                phosphorBleed = 0.5f;
                flickerIntensity = 0.06f;
                flickerSpeed = 18f;
                saturation = 0.7f;
                colorTint = new Color(1f, 0.95f, 0.85f, 1f); // Warm/yellowed
                brightness = 0.95f;
                contrast = 1.2f;
                interlaceIntensity = 0.15f;
                interlaceSpeed = 1.5f;
                jitterIntensity = 0.005f;
                jitterSpeed = 12f;
                wrinkleIntensity = 0.025f;
                wrinkleSpeed = 1.2f;
                wrinkleSize = 0.08f;
                break;

            case VHSPreset.GlitchHeavy:
                // Aggressive digital glitch art
                scanlineIntensity = 0.2f;
                scanlineCount = 300f;
                scanlineSpeed = 2f;
                noiseIntensity = 0.15f;
                noiseSpeed = 25f;
                glitchIntensity = 0.06f;
                glitchSpeed = 12f;
                glitchBlockSize = 0.03f;
                rgbOffsetIntensity = 0.015f;
                colorBleedAmount = 1f;
                phosphorBleed = 0.1f;
                flickerIntensity = 0.08f;
                flickerSpeed = 25f;
                saturation = 1.1f;
                colorTint = new Color(1f, 0.9f, 1f, 1f);
                brightness = 1.1f;
                contrast = 1.3f;
                interlaceIntensity = 0.2f;
                interlaceSpeed = 3f;
                jitterIntensity = 0.008f;
                jitterSpeed = 15f;
                wrinkleIntensity = 0.04f;
                wrinkleSpeed = 2f;
                wrinkleSize = 0.03f;
                break;

            case VHSPreset.Vaporwave:
                // Dreamy pastel vaporwave with strong color grading
                scanlineIntensity = 0.1f;
                scanlineCount = 400f;
                scanlineSpeed = 0.2f;
                noiseIntensity = 0.05f;
                noiseSpeed = 8f;
                glitchIntensity = 0.003f;
                glitchSpeed = 2f;
                glitchBlockSize = 0.06f;
                rgbOffsetIntensity = 0.006f;
                colorBleedAmount = 4f;
                phosphorBleed = 0.4f;
                flickerIntensity = 0.015f;
                flickerSpeed = 8f;
                saturation = 1.2f;
                colorTint = new Color(0.9f, 0.8f, 1.0f, 1f); // Pink/purple tint
                brightness = 1.1f;
                contrast = 1.05f;
                interlaceIntensity = 0.05f;
                interlaceSpeed = 0.5f;
                jitterIntensity = 0.001f;
                jitterSpeed = 4f;
                wrinkleIntensity = 0.003f;
                wrinkleSpeed = 0.4f;
                wrinkleSize = 0.06f;
                break;

            case VHSPreset.SecurityCam:
                // Grainy, desaturated security footage
                scanlineIntensity = 0.4f;
                scanlineCount = 250f;
                scanlineSpeed = 0f;
                noiseIntensity = 0.25f;
                noiseSpeed = 30f;
                glitchIntensity = 0.005f;
                glitchSpeed = 3f;
                glitchBlockSize = 0.1f;
                rgbOffsetIntensity = 0.001f;
                colorBleedAmount = 0.5f;
                phosphorBleed = 0.05f;
                flickerIntensity = 0.04f;
                flickerSpeed = 20f;
                saturation = 0.3f;
                colorTint = new Color(0.9f, 1f, 0.9f, 1f); // Slight green
                brightness = 0.9f;
                contrast = 1.25f;
                interlaceIntensity = 0.2f;
                interlaceSpeed = 1f;
                jitterIntensity = 0.003f;
                jitterSpeed = 10f;
                wrinkleIntensity = 0.015f;
                wrinkleSpeed = 0.5f;
                wrinkleSize = 0.1f;
                break;
        }
    }

    /// <summary>
    /// Smoothly transitions all parameters to a target preset.
    /// </summary>
    public void TransitionToPreset(VHSPreset target, float duration)
    {
        float[] from = CaptureState();
        Color fromTint = colorTint;

        ApplyPreset(target);
        float[] to = CaptureState();
        Color toTint = colorTint;

        RestoreState(from);
        colorTint = fromTint;

        StartCoroutine(LerpToState(to, toTint, duration));
    }

    private float[] CaptureState()
    {
        return new float[]
        {
            scanlineIntensity, scanlineCount, scanlineSpeed,
            noiseIntensity, noiseSpeed,
            glitchIntensity, glitchSpeed, glitchBlockSize,
            rgbOffsetIntensity,
            colorBleedAmount, phosphorBleed,
            flickerIntensity, flickerSpeed,
            saturation, brightness, contrast,
            interlaceIntensity, interlaceSpeed,
            jitterIntensity, jitterSpeed,
            wrinkleIntensity, wrinkleSpeed, wrinkleSize
        };
    }

    private void RestoreState(float[] s)
    {
        scanlineIntensity = s[0];  scanlineCount = s[1];  scanlineSpeed = s[2];
        noiseIntensity = s[3];     noiseSpeed = s[4];
        glitchIntensity = s[5];    glitchSpeed = s[6];    glitchBlockSize = s[7];
        rgbOffsetIntensity = s[8];
        colorBleedAmount = s[9];   phosphorBleed = s[10];
        flickerIntensity = s[11];  flickerSpeed = s[12];
        saturation = s[13];        brightness = s[14];    contrast = s[15];
        interlaceIntensity = s[16]; interlaceSpeed = s[17];
        jitterIntensity = s[18];   jitterSpeed = s[19];
        wrinkleIntensity = s[20];  wrinkleSpeed = s[21];  wrinkleSize = s[22];
    }

    private System.Collections.IEnumerator LerpToState(float[] target, Color targetTint, float duration)
    {
        float[] start = CaptureState();
        Color startTint = colorTint;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            // Lerp every parameter simultaneously
            float[] current = new float[start.Length];
            for (int i = 0; i < start.Length; i++)
            {
                current[i] = Mathf.Lerp(start[i], target[i], t);
            }
            RestoreState(current);

            colorTint = Color.Lerp(startTint, targetTint, t);
            yield return null;
        }

        RestoreState(target);
        colorTint = targetTint;
    }
}
