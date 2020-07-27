using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using OpenSage.Logic.Object;

namespace OpenSage.Diagnostics
{
    internal sealed class InspectorView : DiagnosticView
    {
        public InspectorView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        public override string DisplayName { get; } = "Inspector";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Context.Game.Scene3D.LocalPlayer.SelectedUnits.Count == 0)
            {
                ImGui.Text("Nothing selected");
                return;
            }

            if (Context.Game.Scene3D.LocalPlayer.SelectedUnits.Count > 1)
            {
                ImGui.Text("Multiple objects selected");
                return;
            }

            DrawInspector(Context.Game.Scene3D.LocalPlayer.SelectedUnits.ElementAt(0));
        }

        private void DrawInspector(GameObject gameObject)
        {
            if (ImGui.Button("Kill"))
            {
                // TODO: Time isn't right.
                gameObject.Kill(DeathType.Exploded, Context.Game.MapTime);
            }

            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiUtility.BeginPropertyList();
                ImGuiUtility.PropertyRow("DisplayName", gameObject.Definition.DisplayName);
                ImGuiUtility.PropertyRow("Position", gameObject.Transform.Translation);
                ImGuiUtility.PropertyRow("ModelConditionFlags", gameObject.ModelConditionFlags.DisplayName);
                ImGuiUtility.PropertyRow("Speed", gameObject.Speed);
                ImGuiUtility.PropertyRow("Lift", gameObject.Lift);
                ImGuiUtility.EndPropertyList();
            }

            foreach (var drawModule in gameObject.DrawModules)
            {
                if (ImGui.CollapsingHeader(drawModule.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiUtility.BeginPropertyList();
                    drawModule.DrawInspector();
                    ImGuiUtility.EndPropertyList();
                }
            }

            if (ImGui.CollapsingHeader(gameObject.Body.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGuiUtility.BeginPropertyList();
                ImGuiUtility.PropertyRow("Max health", gameObject.Body.MaxHealth);
                ImGuiUtility.PropertyRow("Health", gameObject.Body.Health);
                ImGuiUtility.EndPropertyList();
            }

            if (gameObject.CurrentWeapon != null)
            {
                var weapon = gameObject.CurrentWeapon;
                if (ImGui.CollapsingHeader("Weapon", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiUtility.BeginPropertyList();
                    ImGuiUtility.PropertyRow("Uses clip", weapon.UsesClip);
                    ImGuiUtility.PropertyRow("Current rounds", weapon.CurrentRounds);
                    ImGuiUtility.PropertyRow("Current target", weapon.CurrentTarget?.TargetType);
                    ImGuiUtility.PropertyRow("Current target position", weapon.CurrentTarget?.TargetPosition);
                    ImGuiUtility.EndPropertyList();
                }
            }

            foreach (var behaviorModule in gameObject.BehaviorModules)
            {
                if (ImGui.CollapsingHeader(behaviorModule.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGuiUtility.BeginPropertyList();
                    behaviorModule.DrawInspector();
                    ImGuiUtility.EndPropertyList();
                }
            }
        }
    }
}
