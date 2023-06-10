using ImGuiNET;
using OpenSage.Graphics.Cameras;

namespace OpenSage.Diagnostics
{
    internal sealed class CameraView : DiagnosticView
    {
        public CameraView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        public override string DisplayName { get; } = "Camera";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            Context.Game.Scene3D.CameraController.DrawInspector();
        }
    }
}
