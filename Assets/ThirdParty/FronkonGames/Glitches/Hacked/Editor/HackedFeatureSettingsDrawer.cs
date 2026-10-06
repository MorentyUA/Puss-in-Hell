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
using UnityEditor;
using static FronkonGames.Glitches.Hacked.Inspector;

namespace FronkonGames.Glitches.Hacked.Editor
{
  /// <summary> Hacked inspector. </summary>
  [CustomPropertyDrawer(typeof(Hacked.Settings))]
  public class HackedFeatureSettingsDrawer : Drawer
  {
    private Hacked.Settings settings;

    protected override void ResetValues() => settings?.ResetDefaultValues();

    protected override void InspectorGUI()
    {
      settings ??= GetSettings<Hacked.Settings>();

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      settings.intensity = Slider("Intensity", "Controls the intensity of the effect [0, 1]. Default 0.", settings.intensity, 0.0f);

      /////////////////////////////////////////////////
      // Hacked.
      /////////////////////////////////////////////////
      Separator();

      settings.strength = Slider("Strength", "Modulates the strength of ALL effects [0, 2]. Default 1.", settings.strength, 0.0f, 2.0f, 1.0f);
      settings.frameJump = Slider("Frame jump", "Intensity of vertical frame jump [0, 1]. Default 0.1.", settings.frameJump, 0.1f);
      IndentLevel++;
      settings.frameJumpSpeed = Slider("Speed", "Vertical frame jump speed[0, 10]. Default 1.", settings.frameJumpSpeed, 0.0f, 10.0f, 1.0f);
      IndentLevel--;
      settings.jitter = Slider("Jitter", "Deformation intensity of horizontal slides [0, 5]. Default 1.", settings.jitter, 0.0f, 5.0f, 1.0f);
      IndentLevel++;
      settings.jitterSpeed = Slider("Speed", "Deformation velocity of horizontal slides [0, 10]. Default 0.2.", settings.jitterSpeed, 0.0f, 10.0f, 0.2f);
      settings.jitterDensity = Slider("Density", "Number of slides [0, 50]. Default 15.", settings.jitterDensity, 0.0f, 50.0f, 15.0f);
      IndentLevel--;
      settings.blocks = Slider("Blocks", "Intensity of block deformation [0, 1]. Default 1.", settings.blocks, 0.5f);
      IndentLevel++;
      settings.blockDensity = Slider("Density", "Block density [0, 50]. Default 10.", settings.blockDensity, 0.0f, 50.0f, 10.0f);
      settings.blockAberration = Vector2Field("Aberration", "Chromatic aberration of block deformation.", settings.blockAberration, Vector2.one);
      settings.blockNoise = Vector2Field("Noise", "Noise applied to the position of the blocks.", settings.blockNoise, Hacked.Settings.DefaultBlockNoise);
      IndentLevel--;
      settings.waves = Slider("Waves", "Intensity of the wave effect [0, 1]. Default 1.", settings.waves);
      IndentLevel++;
      settings.waveSpeed = Slider("Speed", "Wave speed [0, 25]. Default 10.", settings.waveSpeed, 0.0f, 25.0f, 10.0f);
      settings.waveRGBSplit = Slider("RGB split", "Color channel shifts [0, 50]. Default 30.", settings.waveRGBSplit, 0.0f, 50.0f, 30.0f);
      IndentLevel--;
      settings.scanlines = Slider("Scanlines", "Intensity of the scanlines effect [0, 1]. Default 1.", settings.scanlines, 0.2f);
      IndentLevel++;
      settings.scanlinesThreshold = Slider("Threshold", "Range of scanline effect [0, 1]. Default 0.8.", settings.scanlinesThreshold, 0.8f);
      IndentLevel--;
      settings.noise = Slider("Noise", "Noise intensity [0, 1]. Default 0.1.", settings.noise, 0.1f);
      IndentLevel++;
      settings.noiseSpeed = Slider("Speed", "Noise speed [0, 1]. Default 0.1.", settings.noiseSpeed, 0.1f);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Color.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Color") == true)
      {
        IndentLevel++;

        settings.brightness = Slider("Brightness", "Brightness [-1.0, 1.0]. Default 0.", settings.brightness, -1.0f, 1.0f, 0.0f);
        settings.contrast = Slider("Contrast", "Contrast [0.0, 10.0]. Default 1.", settings.contrast, 0.0f, 10.0f, 1.0f);
        settings.gamma = Slider("Gamma", "Gamma [0.1, 10.0]. Default 1.", settings.gamma, 0.01f, 10.0f, 1.0f);
        settings.hue = Slider("Hue", "The color wheel [0.0, 1.0]. Default 0.", settings.hue, 0.0f, 1.0f, 0.0f);
        settings.saturation = Slider("Saturation", "Intensity of a colors [0.0, 2.0]. Default 1.", settings.saturation, 0.0f, 2.0f, 1.0f);

        IndentLevel--;
      }

      /////////////////////////////////////////////////
      // Advanced.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Advanced") == true)
      {
        IndentLevel++;

#if !UNITY_6000_0_OR_NEWER
        settings.filterMode = (FilterMode)EnumPopup("Filter mode", "Filter mode. Default Bilinear.", settings.filterMode, FilterMode.Bilinear);
#endif
        settings.affectSceneView = Toggle("Affect the Scene View?", "Does it affect the Scene View?", settings.affectSceneView);
        settings.whenToInsert = (UnityEngine.Rendering.Universal.RenderPassEvent)EnumPopup("RenderPass event",
          "Render pass injection. Default BeforeRenderingPostProcessing.",
          settings.whenToInsert,
          UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing);
#if !UNITY_6000_0_OR_NEWER
        settings.enableProfiling = Toggle("Enable profiling", "Enable render pass profiling", settings.enableProfiling);
#endif

        IndentLevel--;
      }
    }
  }
}
