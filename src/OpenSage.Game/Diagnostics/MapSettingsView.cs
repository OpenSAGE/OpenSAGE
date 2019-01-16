using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class MapSettingsView : DiagnosticView
    {
        public override string DisplayName { get; } = "Map Settings";

        public MapSettingsView(DiagnosticViewContext context)
            : base(context)
        {

        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Game.Scene3D == null)
            {
                ImGui.Text("Map settings are only available when a map has been loaded.");
                return;
            }

            var showTerrain = Game.Scene3D.ShowTerrain;
            if (ImGui.Checkbox("Show terrain", ref showTerrain))
            {
                Game.Scene3D.ShowTerrain = showTerrain;
            }

            var showWater = Game.Scene3D.ShowWater;
            if (ImGui.Checkbox("Show water", ref showWater))
            {
                Game.Scene3D.ShowWater = showWater;
            }

            var showRoads = Game.Scene3D.ShowRoads;
            if (ImGui.Checkbox("Show roads", ref showRoads))
            {
                Game.Scene3D.ShowRoads = showRoads;
            }

            var showBridges = Game.Scene3D.ShowBridges;
            if (ImGui.Checkbox("Show bridges", ref showBridges))
            {
                Game.Scene3D.ShowBridges = showBridges;
            }

            var showObjects = Game.Scene3D.ShowObjects;
            if (ImGui.Checkbox("Show objects", ref showObjects))
            {
                Game.Scene3D.ShowObjects = showObjects;
            }
        }
    }
}
