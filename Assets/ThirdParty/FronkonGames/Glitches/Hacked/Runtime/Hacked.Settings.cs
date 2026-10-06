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
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Glitches.Hacked
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Hacked
  {
    /// <summary> Settings. </summary>
    [Serializable]
    public sealed class Settings
    {
      public Settings() => ResetDefaultValues();

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Common settings.

      /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
      /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
      public float intensity = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Hacked settings.

      /// <summary> Modulates the strength of ALL effects [0, 2]. Default 1. </summary>
      public float strength = 1.0f;

      /// <summary> Intensity of vertical frame jump [0, 1]. Default 0.1. </summary>
      public float frameJump = 0.1f;

      /// <summary> Vertical frame jump speed [0, 10]. Default 1. </summary>
      public float frameJumpSpeed = 1.0f;

      /// <summary> Deformation intensity of horizontal slides [0, 5]. Default 1. </summary>
      public float jitter = 1.0f;

      /// <summary> Deformation velocity of horizontal slides [0, 10]. Default 0.2. </summary>
      public float jitterSpeed = 0.2f;

      /// <summary> Number of slides [0, 50]. Default 15. </summary>
      public float jitterDensity = 15.0f;

      /// <summary> Intensity of block deformation [0, 1]. Default 1. </summary>
      public float blocks = 0.5f;

      /// <summary> Block density [0, 50]. Default 10. </summary>
      public float blockDensity = 10.0f;

      /// <summary> Chromatic aberration of block deformation. </summary>
      public Vector2 blockAberration = Vector2.one;

      /// <summary> Noise applied to the position of the blocks. </summary>
      public Vector2 blockNoise = DefaultBlockNoise;

      /// <summary> Intensity of the wave effect [0, 1]. Default 1. </summary>
      public float waves = 1.0f;

      /// <summary> Wave speed [0, 25]. Default 10. </summary>
      public float waveSpeed = 10.0f;

      /// <summary> Color channel shifts [0, 50]. Default 30. </summary>
      public float waveRGBSplit = 30.0f;

      /// <summary> Intensity of the scanlines effect [0, 1]. Default 1. </summary>
      public float scanlines = 0.2f;

      /// <summary> Range of scanline effect [0, 1]. Default 0.8. </summary>
      public float scanlinesThreshold = 0.8f;

      /// <summary> Noise intensity [0, 1]. Default 0.1. </summary>
      public float noise = 0.1f;

      /// <summary> Noise speed [0, 1]. Default 0.1. </summary>
      public float noiseSpeed = 0.1f;

      public static Vector2 DefaultBlockNoise = Vector2.one * 0.5f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Color settings.

      /// <summary> Brightness [-1, 1]. Default 0. </summary>
      public float brightness = 0.0f;

      /// <summary> Contrast [0, 10]. Default 1. </summary>
      public float contrast = 1.0f;

      /// <summary> Gamma [0.1, 10]. Default 1. </summary>
      public float gamma = 1.0f;

      /// <summary> The color wheel [0, 1]. Default 0. </summary>
      public float hue = 0.0f;

      /// <summary> Intensity of a colors [0, 2]. Default 1. </summary>
      public float saturation = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Advanced settings.
      /// <summary> Does it affect the Scene View? </summary>
      public bool affectSceneView = false;

#if !UNITY_6000_0_OR_NEWER
      /// <summary> Enable render pass profiling. </summary>
      public bool enableProfiling = false;

      /// <summary> Filter mode. Default Bilinear. </summary>
      public FilterMode filterMode = FilterMode.Bilinear;
#endif

      /// <summary> Render pass injection. Default BeforeRenderingPostProcessing. </summary>
      public RenderPassEvent whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /// <summary> Reset to default values. </summary>
      public void ResetDefaultValues()
      {
        intensity = 1.0f;

        strength = 1.0f;
        frameJump = 0.1f;
        frameJumpSpeed = 1.0f;
        jitter = 1.0f;
        jitterSpeed = 0.2f;
        jitterDensity = 15.0f;
        blocks = 0.5f;
        blockDensity = 10.0f;
        blockAberration = Vector2.one;
        blockNoise = DefaultBlockNoise;
        waves = 1.0f;
        waveSpeed = 10.0f;
        waveRGBSplit = 30.0f;
        scanlines = 0.2f;
        scanlinesThreshold = 0.8f;
        noise = noiseSpeed = 0.1f;

        brightness = 0.0f;
        contrast = 1.0f;
        gamma = 1.0f;
        hue = 0.0f;
        saturation = 1.0f;

        affectSceneView = false;
#if !UNITY_6000_0_OR_NEWER
        enableProfiling = false;
        filterMode = FilterMode.Bilinear;
#endif
        whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
      }
    }
  }
}
