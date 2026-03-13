using UnityEngine;

/// <summary>
/// Runtime controller for the barrel distortion effect in URP.
/// Attach to any GameObject (typically the camera) to control shader parameters.
/// Requires BarrelDistortionFeature to be added to the URP Renderer.
///
/// This script directly updates the shared material, so changes are
/// reflected immediately in the Renderer Feature's pass.
/// </summary>
[ExecuteInEditMode]
public class BarrelDistortionController : MonoBehaviour
{
    [Header("=== Material Reference ===")]
    [Tooltip("Same material assigned to the BarrelDistortionFeature")]
    public Material distortionMaterial;

    [Header("=== Distortion ===")]
    [Range(-1f, 1f)]
    public float distortion = 0.3f;

    [Range(-1f, 1f)]
    public float cubicDistortion = 0.1f;

    [Range(0.5f, 2f)]
    public float zoom = 1.05f;

    [Header("=== Chromatic Aberration ===")]
    [Range(0f, 0.05f)]
    public float chromaticAberration = 0.005f;

    [Header("=== Vignette ===")]
    [Range(0f, 2f)]
    public float vignetteStrength = 0.5f;

    [Range(0f, 2f)]
    public float vignetteRadius = 0.8f;

    [Range(0.01f, 1f)]
    public float vignetteSoftness = 0.5f;

    [Header("=== CRT Curvature ===")]
    [Range(0f, 1f)]
    public float screenCurvature = 0f;

    [Range(0f, 1f)]
    public float cornerDarkness = 0.3f;

    [Header("=== Background ===")]
    public Color backgroundColor = Color.black;

    [Header("=== Presets ===")]
    [Tooltip("Apply a preset configuration on start")]
    public DistortionPreset preset = DistortionPreset.None;

    public enum DistortionPreset
    {
        None,
        SubtleFisheye,
        RetroArcade,
        CRTMonitor,
        SecurityCamera,
        DreamSequence
    }

    private void OnEnable()
    {
        if (preset != DistortionPreset.None)
        {
            ApplyPreset(preset);
        }
    }

    private void Update()
    {
        PushParametersToMaterial();
    }

    /// <summary>
    /// Sends all current parameter values to the shader material.
    /// Called every frame to allow real-time tweaking in the Inspector.
    /// </summary>
    private void PushParametersToMaterial()
    {
        if (distortionMaterial == null) return;

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
    }

    /// <summary>
    /// Applies a named preset to all distortion parameters.
    /// Call this from code or hook it to a UI button.
    /// </summary>
    public void ApplyPreset(DistortionPreset targetPreset)
    {
        switch (targetPreset)
        {
            case DistortionPreset.SubtleFisheye:
                // Light barrel distortion, suitable for most games
                distortion = 0.15f;
                cubicDistortion = 0.05f;
                zoom = 1.02f;
                chromaticAberration = 0.002f;
                vignetteStrength = 0.3f;
                vignetteRadius = 0.9f;
                vignetteSoftness = 0.4f;
                screenCurvature = 0f;
                cornerDarkness = 0.1f;
                break;

            case DistortionPreset.RetroArcade:
                // Strong distortion matching your reference image
                distortion = 0.35f;
                cubicDistortion = 0.15f;
                zoom = 1.1f;
                chromaticAberration = 0.008f;
                vignetteStrength = 0.7f;
                vignetteRadius = 0.75f;
                vignetteSoftness = 0.5f;
                screenCurvature = 0.15f;
                cornerDarkness = 0.5f;
                break;

            case DistortionPreset.CRTMonitor:
                // Heavy CRT curvature with scanline-friendly settings
                distortion = 0.25f;
                cubicDistortion = 0.1f;
                zoom = 1.08f;
                chromaticAberration = 0.01f;
                vignetteStrength = 0.8f;
                vignetteRadius = 0.7f;
                vignetteSoftness = 0.6f;
                screenCurvature = 0.25f;
                cornerDarkness = 0.6f;
                break;

            case DistortionPreset.SecurityCamera:
                // Wide-angle lens feel
                distortion = 0.5f;
                cubicDistortion = 0.2f;
                zoom = 1.15f;
                chromaticAberration = 0.003f;
                vignetteStrength = 0.4f;
                vignetteRadius = 0.85f;
                vignetteSoftness = 0.3f;
                screenCurvature = 0f;
                cornerDarkness = 0.2f;
                break;

            case DistortionPreset.DreamSequence:
                // Pulsing, dreamy barrel distortion
                distortion = 0.2f;
                cubicDistortion = 0.3f;
                zoom = 1.05f;
                chromaticAberration = 0.015f;
                vignetteStrength = 1.0f;
                vignetteRadius = 0.6f;
                vignetteSoftness = 0.7f;
                screenCurvature = 0.1f;
                cornerDarkness = 0.4f;
                break;
        }
    }

    /// <summary>
    /// Smoothly transitions all parameters to a target preset over time.
    /// </summary>
    public void TransitionToPreset(DistortionPreset targetPreset, float duration)
    {
        // Capture current state
        var from = CaptureState();

        // Apply target preset to get target values
        ApplyPreset(targetPreset);
        var to = CaptureState();

        // Restore current values before starting the lerp
        RestoreState(from);

        StartCoroutine(LerpToState(to, duration));
    }

    private float[] CaptureState()
    {
        return new float[]
        {
            distortion, cubicDistortion, zoom,
            chromaticAberration,
            vignetteStrength, vignetteRadius, vignetteSoftness,
            screenCurvature, cornerDarkness
        };
    }

    private void RestoreState(float[] state)
    {
        distortion = state[0];
        cubicDistortion = state[1];
        zoom = state[2];
        chromaticAberration = state[3];
        vignetteStrength = state[4];
        vignetteRadius = state[5];
        vignetteSoftness = state[6];
        screenCurvature = state[7];
        cornerDarkness = state[8];
    }

    private System.Collections.IEnumerator LerpToState(float[] target, float duration)
    {
        float[] start = CaptureState();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            distortion = Mathf.Lerp(start[0], target[0], t);
            cubicDistortion = Mathf.Lerp(start[1], target[1], t);
            zoom = Mathf.Lerp(start[2], target[2], t);
            chromaticAberration = Mathf.Lerp(start[3], target[3], t);
            vignetteStrength = Mathf.Lerp(start[4], target[4], t);
            vignetteRadius = Mathf.Lerp(start[5], target[5], t);
            vignetteSoftness = Mathf.Lerp(start[6], target[6], t);
            screenCurvature = Mathf.Lerp(start[7], target[7], t);
            cornerDarkness = Mathf.Lerp(start[8], target[8], t);

            yield return null;
        }

        RestoreState(target);
    }
}
