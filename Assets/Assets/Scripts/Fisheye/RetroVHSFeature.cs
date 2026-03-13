using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

/// <summary>
/// URP Renderer Feature for the RetroVHS CRT/VHS post-processing effect.
/// Uses the Unity 6 Render Graph API (AddBlitPass).
///
/// SETUP:
/// 1. Create a Material using "Custom/RetroVHS" shader.
/// 2. On your URP Renderer Asset → Add Renderer Feature → RetroVHS Feature.
/// 3. Assign the material.
/// 4. Attach RetroVHSController to your camera for runtime control + presets.
/// 5. This pass runs AFTER barrel distortion so both effects stack.
/// </summary>
public class RetroVHSFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Material using the Custom/RetroVHS shader")]
        public Material vhsMaterial = null;

        [Tooltip("Injection point. Use AfterRenderingPostProcessing to capture everything.")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public Settings settings = new Settings();
    private RetroVHSPass _pass;

    public override void Create()
    {
        _pass = new RetroVHSPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.vhsMaterial == null) return;
        renderer.EnqueuePass(_pass);
    }

    private class RetroVHSPass : ScriptableRenderPass
    {
        private readonly Settings _settings;

        public RetroVHSPass(Settings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_settings.vhsMaterial == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (!resourceData.activeColorTexture.IsValid()) return;

            TextureHandle source = resourceData.activeColorTexture;

            // Build destination texture
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            RenderTextureDescriptor rtDesc = cameraData.cameraTargetDescriptor;
            rtDesc.depthBufferBits = 0;
            rtDesc.msaaSamples = 1;

            TextureDesc destDesc = new TextureDesc(rtDesc.width, rtDesc.height);
            destDesc.colorFormat = rtDesc.graphicsFormat;
            destDesc.name = "_RetroVHSDest";
            destDesc.clearBuffer = false;
            destDesc.depthBufferBits = DepthBits.None;
            destDesc.msaaSamples = MSAASamples.None;
            destDesc.filterMode = FilterMode.Bilinear;
            destDesc.wrapMode = TextureWrapMode.Clamp;

            TextureHandle destination = renderGraph.CreateTexture(destDesc);

            // Blit through VHS material
            var blitParams = new RenderGraphUtils.BlitMaterialParameters(
                source, destination, _settings.vhsMaterial, 0
            );

            renderGraph.AddBlitPass(blitParams, passName: "RetroVHS");

            // Redirect camera output
            resourceData.cameraColor = destination;
        }
    }
}
