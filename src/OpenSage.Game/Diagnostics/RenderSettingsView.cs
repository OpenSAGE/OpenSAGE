using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering.Shadows;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class RenderSettingsView : DiagnosticView
    {
        private readonly Dictionary<Tuple<Texture, int>, TextureView> _cachedTextureViews = new Dictionary<Tuple<Texture, int>, TextureView>();

        private readonly List<uint> _shadowMapSizes;
        private readonly string[] _shadowMapSizeNames;

        public override string DisplayName { get; } = "Render Settings";

        public RenderSettingsView(DiagnosticViewContext context)
            : base(context)
        {
            _shadowMapSizes = new List<uint> { 256u, 512u, 1024u, 2048u };
            _shadowMapSizeNames = _shadowMapSizes.Select(x => x.ToString()).ToArray();
    }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Game.Scene3D == null)
            {
                ImGui.Text("Render settings are only available when a map has been loaded.");
                return;
            }

            var lighting = Game.Scene3D.Lighting;

            ImGui.Text("Time of day");
            {
                foreach (var timeOfDay in GetTimesOfDay())
                {
                    if (ImGui.RadioButton(timeOfDay.ToString(), lighting.TimeOfDay == timeOfDay))
                    {
                        lighting.TimeOfDay = timeOfDay;
                    }
                }
            }

            ImGui.Separator();

            var shadowSettings = Game.Scene3D.Shadows;

            ImGui.Text("Shadows");
            {
                foreach (var shadowsType in GetShadowsTypes())
                {
                    if (ImGui.RadioButton(shadowsType.ToString(), shadowSettings.ShadowsType == shadowsType))
                    {
                        shadowSettings.ShadowsType = shadowsType;
                    }
                }
            }

            if (shadowSettings.ShadowsType != ShadowsType.None)
            {
                ImGui.Separator();

                var currentShadowMapSizeIndex = _shadowMapSizes.IndexOf(shadowSettings.ShadowMapSize);
                ImGui.Combo("Shadow map size", ref currentShadowMapSizeIndex, _shadowMapSizeNames, _shadowMapSizes.Count);
                shadowSettings.ShadowMapSize = _shadowMapSizes[currentShadowMapSizeIndex];

                ImGui.Text("Shadow cascades");
                {
                    foreach (var cascadeType in GetShadowCascades())
                    {
                        if (ImGui.RadioButton(cascadeType.ToString(), shadowSettings.ShadowMapCascades == cascadeType))
                        {
                            shadowSettings.ShadowMapCascades = cascadeType;
                        }
                    }
                }

                ImGui.Separator();

                var shadowDistance = shadowSettings.ShadowDistance;
                if (ImGui.SliderFloat("Shadow distance", ref shadowDistance, 10, 2000))
                {
                    shadowSettings.ShadowDistance = shadowDistance;
                }

                var stabilizeShadows = shadowSettings.StabilizeShadowCascades;
                if (ImGui.Checkbox("Stabilize shadows", ref stabilizeShadows))
                {
                    shadowSettings.StabilizeShadowCascades = stabilizeShadows;
                }

                var visualizeCascades = shadowSettings.VisualizeCascades;
                if (ImGui.Checkbox("Visualize shadow cascades", ref visualizeCascades))
                {
                    shadowSettings.VisualizeCascades = visualizeCascades;
                }

                if (Game.Graphics.ShadowMap != null)
                {
                    for (var i = 0; i < (int) Game.Graphics.ShadowMap.ArrayLayers; i++)
                    {
                        var shadowMapTuple = Tuple.Create(Game.Graphics.ShadowMap, i);
                        if (!_cachedTextureViews.TryGetValue(shadowMapTuple, out var shadowMapView))
                        {
                            shadowMapView = AddDisposable(Game.GraphicsDevice.ResourceFactory.CreateTextureView(
                                new TextureViewDescription(Game.Graphics.ShadowMap, 0, 1, (uint) i, 1)));
                            _cachedTextureViews.Add(shadowMapTuple, shadowMapView);
                        }

                        var imagePointer = ImGuiRenderer.GetOrCreateImGuiBinding(
                            Game.GraphicsDevice.ResourceFactory,
                            shadowMapView);

                        ImGui.Image(
                            imagePointer,
                            new Vector2(250, 250),
                            Game.GetTopLeftUV(),
                            Game.GetBottomRightUV(),
                            Vector4.One);
                    }
                }
            }

            ImGui.Separator();

            var enableCloudShadows = lighting.EnableCloudShadows;
            if (ImGui.Checkbox("Enable cloud shadows", ref enableCloudShadows))
            {
                lighting.EnableCloudShadows = enableCloudShadows;
            }
        }

        private static IEnumerable<TimeOfDay> GetTimesOfDay()
        {
            yield return TimeOfDay.Morning;
            yield return TimeOfDay.Afternoon;
            yield return TimeOfDay.Evening;
            yield return TimeOfDay.Night;
        }

        private static IEnumerable<ShadowsType> GetShadowsTypes()
        {
            yield return ShadowsType.None;
            yield return ShadowsType.Hard;
            yield return ShadowsType.Soft;
        }

        private static IEnumerable<ShadowMapCascades> GetShadowCascades()
        {
            yield return ShadowMapCascades.OneCascade;
            yield return ShadowMapCascades.TwoCascades;
            yield return ShadowMapCascades.FourCascades;
        }
    }
}
