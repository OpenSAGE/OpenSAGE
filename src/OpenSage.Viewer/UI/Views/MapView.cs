using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Scripting;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class MapView : GameView
    {
        private readonly Game _game;
        private readonly StringBuilder _scriptStateContent;

        private readonly Dictionary<Tuple<Texture, int>, TextureView> _cachedTextureViews = new Dictionary<Tuple<Texture, int>, TextureView>();

        public MapView(AssetViewContext context)
            : base(context)
        {
            _game = context.Game;
            _game.Scene3D = _game.ContentManager.Load<Scene3D>(context.Entry.FilePath);

            _scriptStateContent = new StringBuilder();

            _game.Scripting.OnUpdateFinished += OnScriptingUpdateFinished;

            AddDisposeAction(() => _game.Scripting.OnUpdateFinished -= OnScriptingUpdateFinished);
        }

        private void OnScriptingUpdateFinished(object sender, ScriptingSystem scripting)
        {
            _scriptStateContent.Clear();

            _scriptStateContent.AppendLine("Counters:");

            foreach (var kv in scripting.Counters)
            {
                _scriptStateContent.AppendFormat("  {0}: {1}\n", kv.Key, kv.Value);
            }

            _scriptStateContent.AppendLine("Flags:");

            foreach (var kv in scripting.Flags)
            {
                _scriptStateContent.AppendFormat("  {0}: {1}\n", kv.Key, kv.Value);
            }

            _scriptStateContent.AppendLine("Timers:");

            foreach (var kv in scripting.Timers)
            {
                _scriptStateContent.AppendFormat("  {0}: {1} ({2})\n", kv.Key, kv.Value, scripting.Counters[kv.Key]);
            }
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("map panels", new Vector2(350, 0), true, 0);

            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var showTerrain = _game.Scene3D.ShowTerrain;
                if (ImGui.Checkbox("Show terrain", ref showTerrain))
                {
                    _game.Scene3D.ShowTerrain = showTerrain;
                }

                var showWater = _game.Scene3D.ShowWater;
                if (ImGui.Checkbox("Show water", ref showWater))
                {
                    _game.Scene3D.ShowWater = showWater;
                }

                var showRoads = _game.Scene3D.ShowRoads;
                if (ImGui.Checkbox("Show roads", ref showRoads))
                {
                    _game.Scene3D.ShowRoads = showRoads;
                }

                var showObjects = _game.Scene3D.ShowObjects;
                if (ImGui.Checkbox("Show objects", ref showObjects))
                {
                    _game.Scene3D.ShowObjects = showObjects;
                }

                ImGui.Separator();

                var enableMacroTexture = _game.Scene3D.Terrain.EnableMacroTexture;
                if (ImGui.Checkbox("Enable macro texture", ref enableMacroTexture))
                {
                    _game.Scene3D.Terrain.EnableMacroTexture = enableMacroTexture;
                }
            }

            if (ImGui.CollapsingHeader("Lighting", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var lighting = _game.Scene3D.Lighting;

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

                var shadowSettings = _game.Scene3D.Shadows;

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

                    var shadowMapSizes = new List<uint> { 256, 512, 1024, 2048 };
                    var currentShadowMapSizeIndex = shadowMapSizes.IndexOf(shadowSettings.ShadowMapSize);
                    ImGui.Combo("Shadow map size", ref currentShadowMapSizeIndex, shadowMapSizes.Select(x => x.ToString()).ToArray(), shadowMapSizes.Count);
                    shadowSettings.ShadowMapSize = shadowMapSizes[currentShadowMapSizeIndex];

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

                    if (_game.Graphics.ShadowMap != null)
                    {
                        for (var i = 0; i < (int) _game.Graphics.ShadowMap.ArrayLayers; i++)
                        {
                            var shadowMapTuple = Tuple.Create(_game.Graphics.ShadowMap, i);
                            if (!_cachedTextureViews.TryGetValue(shadowMapTuple, out var shadowMapView))
                            {
                                shadowMapView = AddDisposable(_game.GraphicsDevice.ResourceFactory.CreateTextureView(
                                    new TextureViewDescription(_game.Graphics.ShadowMap, 0, 1, (uint) i, 1)));
                                _cachedTextureViews.Add(shadowMapTuple, shadowMapView);
                            }

                            var imagePointer = Context.ImGuiRenderer.GetOrCreateImGuiBinding(
                                Context.GraphicsDevice.ResourceFactory,
                                shadowMapView);

                            ImGui.Image(
                                imagePointer,
                                new Vector2(250, 250),
                                GetTopLeftUV(),
                                GetBottomRightUV(),
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

            if (ImGui.CollapsingHeader("Scripts", 0))
            {
                if (ImGui.Button(_game.Scripting.Active ? "Stop Scripts" : "Start Scripts"))
                {
                    _game.Scripting.Active = !_game.Scripting.Active;
                }

                ImGui.Separator();

                ImGui.Text(_scriptStateContent.ToString());

                ImGui.Separator();

                var scriptsList = _game.Scene3D.MapFile.GetPlayerScriptsList();
                for (var i = 0; i < scriptsList.ScriptLists.Length; i++)
                {
                    DrawScriptList(i, scriptsList.ScriptLists[i]);
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
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

        private static void DrawScriptList(int index, ScriptList scriptList)
        {
            if (ImGui.TreeNodeEx($"Script List {index}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var childGroup in scriptList.ScriptGroups)
                {
                    DrawScriptGroup(childGroup);
                }

                foreach (var script in scriptList.Scripts)
                {
                    DrawScript(script);
                }

                ImGui.TreePop();
            }
        }

        private static void DrawScriptGroup(ScriptGroup scriptGroup)
        {
            if (ImGui.TreeNodeEx(GetScriptDetails(scriptGroup.Name, scriptGroup.IsActive, scriptGroup.IsSubroutine), ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var childGroup in scriptGroup.Groups)
                {
                    DrawScriptGroup(childGroup);
                }

                foreach (var script in scriptGroup.Scripts)
                {
                    DrawScript(script);
                }

                ImGui.TreePop();
            }
        }

        private static void DrawScript(Script script)
        {
            ImGui.BulletText(GetScriptDetails(script.Name, script.IsActive, script.IsSubroutine));
        }

        private static string GetScriptDetails(string name, bool isActive, bool isSubroutine)
        {
            var result = name;

            result += isActive ? " (Active" : "(Inactive";

            if (isSubroutine)
            {
                result += ", Subroutine";
            }

            result += ")";

            return result;
        }
    }
}
