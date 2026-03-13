using UnityEngine;

/// <summary>
/// Applies a barrel distortion (fisheye) post-processing effect to the camera output.
/// Affects both the 3D scene and UI when the Canvas is set to Screen Space - Camera.
/// 
/// SETUP (Built-in Render Pipeline):
/// 1. Attach this script to your Main Camera.
/// 2. Create a Material using the "Custom/BarrelDistortion" shader.
/// 3. Assign that material to the "Distortion Material" field.
/// 4. Set your UI Canvas to "Screen Space - Camera" and assign the same camera.
/// 5. Adjust parameters in the Inspector.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BarrelDistortionEffect : MonoBehaviour
{
    [Header("=== Material ===")]
    [Tooltip("Material using the Custom/BarrelDistortion shader")]
    public Material distortionMaterial;

    [Header("=== Distortion Settings ===")]
    [Tooltip("Main barrel distortion strength. Positive = barrel, Negative = pincushion")]
    [Range(-1f, 1f)]
    public float distortion = 0.3f;

    [Tooltip("Higher-order cubic distortion for more pronounced edges")]
    [Range(-1f, 1f)]
    public float cubicDistortion = 0.1f;

    [Tooltip("Zoom factor to compensate black edges caused by distortion")]
    [Range(0.5f, 2f)]
    public float zoom = 1.05f;

    [Header("=== Chromatic Aberration ===")]
    [Tooltip("Strength of RGB channel separation at the edges")]
    [Range(0f, 0.05f)]
    public float chromaticAberration = 0.005f;

    [Header("=== Vignette ===")]
    [Tooltip("Intensity of the edge darkening vignette")]
    [Range(0f, 2f)]
    public float vignetteStrength = 0.5f;

    [Tooltip("Radius before vignette starts")]
    [Range(0f, 2f)]
    public float vignetteRadius = 0.8f;

    [Tooltip("Softness of the vignette transition")]
    [Range(0.01f, 1f)]
    public float vignetteSoftness = 0.5f;

    [Header("=== CRT Screen Curvature ===")]
    [Tooltip("Additional CRT-like curvature applied before distortion")]
    [Range(0f, 1f)]
    public float screenCurvature = 0f;

    [Tooltip("How dark the corners get (CRT bezel effect)")]
    [Range(0f, 1f)]
    public float cornerDarkness = 0.3f;

    [Header("=== Background ===")]
    [Tooltip("Color shown in areas outside the distorted image")]
    public Color backgroundColor = Color.black;

    [Header("=== Animation (Optional) ===")]
    [Tooltip("Enable breathing animation on distortion strength")]
    public bool animateDistortion = false;

    [Tooltip("Speed of the breathing animation")]
    public float animationSpeed = 1f;

    [Tooltip("Amplitude of the animation oscillation")]
    [Range(0f, 0.2f)]
    public float animationAmplitude = 0.05f;

    // Cached base distortion value for animation
    private float _baseDistortion;

    private void OnEnable()
    {
        _baseDistortion = distortion;
    }

    private void Update()
    {
        // Optional breathing animation
        if (animateDistortion)
        {
            distortion = _baseDistortion + Mathf.Sin(Time.time * animationSpeed) * animationAmplitude;
        }
    }

    /// <summary>
    /// Called by Unity after the camera finishes rendering.
    /// Applies the barrel distortion as a full-screen image effect.
    /// </summary>
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (distortionMaterial == null)
        {
            Debug.LogWarning("[BarrelDistortion] No distortion material assigned. Passing through.");
            Graphics.Blit(source, destination);
            return;
        }

        // Transfer all parameters to the shader
        distortionMaterial.SetFloat("_Distortion", distortion);
        distortionMaterial.SetFloat("_CubicDistortion", cubicDistortion);
        distortionMaterial.SetFloat("_Zoom", zoom);
        distortionMaterial.SetFloat("_ChromaticAberration", chromaticAberration);
        distortionMaterial.SetFloat("_VignetteStrength", vignetteStrength);
        distortionMaterial.SetFloat("_VignetteRadius", vignetteRadius);
        distortionMaterial.SetFloat("_VignetteSoftness", vignetteSoftness);
        distortionMaterial.SetFloat("_ScreenCurvature", screenCurvature);
        distortionMaterial.SetFloat("_CornerDarkness", cornerDarkness);
        distortionMaterial.SetColor("_BackgroundColor", backgroundColor);

        // Apply the full-screen distortion pass
        Graphics.Blit(source, destination, distortionMaterial, 0);
    }

    /// <summary>
    /// Smoothly transitions distortion parameters over time.
    /// Useful for impact effects, transitions, or cutscenes.
    /// </summary>
    public void AnimateToSettings(float targetDistortion, float targetZoom, float duration)
    {
        StartCoroutine(LerpSettings(targetDistortion, targetZoom, duration));
    }

    private System.Collections.IEnumerator LerpSettings(float targetDistortion, float targetZoom, float duration)
    {
        float startDistortion = distortion;
        float startZoom = zoom;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            distortion = Mathf.Lerp(startDistortion, targetDistortion, t);
            zoom = Mathf.Lerp(startZoom, targetZoom, t);
            yield return null;
        }

        distortion = targetDistortion;
        zoom = targetZoom;
    }

    /// <summary>
    /// Resets all distortion parameters to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        distortion = 0.3f;
        cubicDistortion = 0.1f;
        zoom = 1.05f;
        chromaticAberration = 0.005f;
        vignetteStrength = 0.5f;
        vignetteRadius = 0.8f;
        vignetteSoftness = 0.5f;
        screenCurvature = 0f;
        cornerDarkness = 0.3f;
        backgroundColor = Color.black;
    }
}
