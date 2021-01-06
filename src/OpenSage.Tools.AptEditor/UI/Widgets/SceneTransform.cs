using ImGuiNET;
using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class SceneTransform : IWidget
    {
        public void Draw(AptSceneManager manager)
        {
            if (ImGui.Begin("Scene transform", ImGuiWindowFlags.AlwaysAutoResize))
            {
                var scale = manager.CurrentScale;
                var scaleFactor = MathF.Log(scale, 2);
                ImGui.SliderFloat("Scale Exponent", ref scaleFactor, -4, 4);
                scale = MathF.Pow(2, scaleFactor);
                ImGui.InputFloat("Scale (does not work for now)", ref scale);
                ImGui.Spacing();

                var windowSize = manager.Game.Window.ClientBounds;
                var offset = manager.CurrentOffset;
                var x = offset.X;
                var y = offset.Y;
                ImGui.SliderFloat("SceneView Offset X", ref x, MathF.Min(0, x) - 100, MathF.Max(windowSize.Width, x) + 100);
                ImGui.SliderFloat("SceneView Offset Y", ref y, MathF.Min(0, y) - 100, MathF.Max(windowSize.Height, y) + 100);
                ImGui.Spacing();

                var previousColor = manager.DisplayBackgroundColor;
                var color = new Vector3(previousColor.R, previousColor.G, previousColor.B);
                ImGui.ColorEdit3("Background (display only)", ref color);

                manager.CurrentScale = scale;
                manager.CurrentOffset = new Vector2(x, y);
                manager.ChangeDisplayBackgroundColor(new ColorRgbaF(color.X, color.Y, color.Z, 1));
            }
            ImGui.End();
        }
    }
}
