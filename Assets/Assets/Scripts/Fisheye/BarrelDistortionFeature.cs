using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

/// <summary>
/// URP Scriptable Renderer Feature — barrel distortion full-screen blit.
/// Written for Unity 6+ Render Graph API (RecordRenderGraph + AddBlitPass).
/// </summary>
public class BarrelDistortionFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Material using the Custom/BarrelDistortion shader")]
        public Material distortionMaterial = null;

        [Tooltip("When to inject this pass. Use AfterRenderingPostProcessing to capture UI.")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public Settings settings = new Settings();
    private BarrelDistortionPass _pass;

    public override void Create()
    {
        _pass = new BarrelDistortionPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.distortionMaterial == null) return;
        renderer.EnqueuePass(_pass);
    }

    /// <summary>
    /// Render Graph pass. Uses AddBlitPass to blit the camera color through the distortion material.
    /// </summary>
    private class BarrelDistortionPass : ScriptableRenderPass
    {
        private readonly Settings _settings;

        public BarrelDistortionPass(Settings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;
            // Inform URP that this pass requires access to the color buffer
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_settings.distortionMaterial == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // Safety check: skip if source texture is not valid
            if (!resourceData.activeColorTexture.IsValid()) return;

            TextureHandle source = resourceData.activeColorTexture;

            // Build destination texture descriptor from the camera target
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            RenderTextureDescriptor rtDesc = cameraData.cameraTargetDescriptor;
            rtDesc.depthBufferBits = 0;
            rtDesc.msaaSamples = 1;

            TextureDesc destDesc = new TextureDesc(rtDesc.width, rtDesc.height);
            destDesc.colorFormat = rtDesc.graphicsFormat;
            destDesc.name = "_BarrelDistortionDest";
            destDesc.clearBuffer = false;
            destDesc.depthBufferBits = DepthBits.None;
            destDesc.msaaSamples = MSAASamples.None;
            destDesc.filterMode = FilterMode.Bilinear;
            destDesc.wrapMode = TextureWrapMode.Clamp;

            TextureHandle destination = renderGraph.CreateTexture(destDesc);

            var blitParams = new RenderGraphUtils.BlitMaterialParameters(
                source, destination, _settings.distortionMaterial, 0
            );

            renderGraph.AddBlitPass(blitParams, passName: "BarrelDistortion");

            // Redirect camera output to our distorted texture (avoids blitting back)
            resourceData.cameraColor = destination;
        }
    }
}