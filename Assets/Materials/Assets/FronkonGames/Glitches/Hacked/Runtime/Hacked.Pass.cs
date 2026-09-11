////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

namespace FronkonGames.Glitches.Hacked
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Hacked
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private readonly Settings settings;

#if UNITY_6000_0_OR_NEWER
#else
      private RenderTargetIdentifier colorBuffer;
      private RenderTextureDescriptor renderTextureDescriptor;

      private readonly int renderTextureHandle0 = Shader.PropertyToID($"{Constants.Asset.AssemblyName}.RTH0");

      private const string CommandBufferName = Constants.Asset.AssemblyName;

      private ProfilingScope profilingScope;
      private readonly ProfilingSampler profilingSamples = new(Constants.Asset.AssemblyName);
#endif

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");

        internal static readonly int Strength = Shader.PropertyToID("_Strength");
        internal static readonly int FrameJump = Shader.PropertyToID("_FrameJump");
        internal static readonly int FrameJumpSpeed = Shader.PropertyToID("_FrameJumpSpeed");
        internal static readonly int Jitter = Shader.PropertyToID("_Jitter");
        internal static readonly int JitterSpeed = Shader.PropertyToID("_JitterSpeed");
        internal static readonly int JitterDensity = Shader.PropertyToID("_JitterDensity");
        internal static readonly int Blocks = Shader.PropertyToID("_Blocks");
        internal static readonly int BlockDensity = Shader.PropertyToID("_BlockDensity");
        internal static readonly int BlockAberration = Shader.PropertyToID("_BlockAberration");
        internal static readonly int BlockNoise = Shader.PropertyToID("_BlockNoise");
        internal static readonly int Waves = Shader.PropertyToID("_Waves");
        internal static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        internal static readonly int WaveRGBSplit = Shader.PropertyToID("_WaveRGBSplit");
        internal static readonly int Scanlines = Shader.PropertyToID("_Scanlines");
        internal static readonly int ScanlinesThreshold = Shader.PropertyToID("_ScanlinesThreshold");
        internal static readonly int Noise = Shader.PropertyToID("_Noise");
        internal static readonly int NoiseSpeed = Shader.PropertyToID("_NoiseSpeed");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass(Settings settings) : base()
      {
        this.settings = settings;
#if UNITY_6000_0_OR_NEWER
        profilingSampler = new ProfilingSampler(Constants.Asset.AssemblyName);
#endif
      }

      private void UpdateMaterial()
      {
        material.shaderKeywords = null;
        material.SetFloat(ShaderIDs.Intensity, settings.intensity);

        material.SetFloat(ShaderIDs.Strength, settings.strength);
        material.SetFloat(ShaderIDs.FrameJump, settings.frameJump * 0.1f);
        material.SetFloat(ShaderIDs.FrameJumpSpeed, settings.frameJumpSpeed);
        material.SetFloat(ShaderIDs.Jitter, settings.jitter * 10.0f);
        material.SetFloat(ShaderIDs.JitterSpeed, settings.jitterSpeed);
        material.SetFloat(ShaderIDs.JitterDensity, settings.jitterDensity);
        material.SetFloat(ShaderIDs.Blocks, settings.blocks);
        material.SetFloat(ShaderIDs.BlockDensity, settings.blockDensity);
        material.SetVector(ShaderIDs.BlockAberration, settings.blockAberration);
        material.SetVector(ShaderIDs.BlockNoise, settings.blockNoise * 0.1f);
        material.SetFloat(ShaderIDs.Waves, settings.waves);
        material.SetFloat(ShaderIDs.WaveSpeed, settings.waveSpeed);
        material.SetFloat(ShaderIDs.WaveRGBSplit, settings.waveRGBSplit);
        material.SetFloat(ShaderIDs.Scanlines, settings.scanlines * 0.01f);
        material.SetFloat(ShaderIDs.ScanlinesThreshold, Mathf.Min(settings.scanlinesThreshold, 0.99f));
        material.SetFloat(ShaderIDs.Noise, settings.noise);
        material.SetFloat(ShaderIDs.NoiseSpeed, settings.noiseSpeed);

        material.SetFloat(ShaderIDs.Brightness, settings.brightness);
        material.SetFloat(ShaderIDs.Contrast, settings.contrast);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / settings.gamma);
        material.SetFloat(ShaderIDs.Hue, settings.hue);
        material.SetFloat(ShaderIDs.Saturation, settings.saturation);
      }

#if UNITY_6000_0_OR_NEWER
      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        if (material == null || settings.intensity == 0.0f)
          return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer == true)
          return;

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (cameraData.camera.cameraType == CameraType.SceneView && settings.affectSceneView == false || cameraData.postProcessEnabled == false)
          return;

        TextureHandle source = resourceData.activeColorTexture;
        TextureHandle destination = renderGraph.CreateTexture(source.GetDescriptor(renderGraph));

        UpdateMaterial();

        RenderGraphUtils.BlitMaterialParameters pass = new(source, destination, material, 0);
        renderGraph.AddBlitPass(pass, $"{Constants.Asset.AssemblyName}.Pass");

        resourceData.cameraColor = destination;
      }
#elif UNITY_2022_3_OR_NEWER
      /// <inheritdoc/>
      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        renderTextureDescriptor.depthBufferBits = 0;

        colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
        cmd.GetTemporaryRT(renderTextureHandle0, renderTextureDescriptor, settings.filterMode);
      }

      /// <inheritdoc/>
      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (material == null ||
            renderingData.postProcessingEnabled == false ||
            settings.intensity <= 0.0f ||
            settings.affectSceneView == false && renderingData.cameraData.isSceneViewCamera == true)
          return;

        CommandBuffer cmd = CommandBufferPool.Get(CommandBufferName);

        if (settings.enableProfiling == true)
          profilingScope = new ProfilingScope(cmd, profilingSamples);

        UpdateMaterial();

        cmd.Blit(colorBuffer, renderTextureHandle0, material);
        cmd.Blit(renderTextureHandle0, colorBuffer, material);

        cmd.ReleaseTemporaryRT(renderTextureHandle0);

        if (settings.enableProfiling == true)
          profilingScope.Dispose();

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }

      public override void OnCameraCleanup(CommandBuffer cmd) => cmd.ReleaseTemporaryRT(renderTextureHandle0);
#else
      #error Unsupported Unity version. Please update to a newer version of Unity.
#endif
    }
  }
}
