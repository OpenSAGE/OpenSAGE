using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class PartitionView : DiagnosticView
    {
        public override string DisplayName { get; } = "Partition";

        public PartitionView(DiagnosticViewContext context)
            : base(context)
        {

        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            Context.Game.Scene3D?.PartitionCellManager.DrawDiagnostic();
        }
    }
}
