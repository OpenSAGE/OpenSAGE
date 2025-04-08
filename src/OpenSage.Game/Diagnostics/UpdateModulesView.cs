namespace OpenSage.Diagnostics;

internal sealed class UpdateModulesView : DiagnosticView
{
    public UpdateModulesView(DiagnosticViewContext context)
        : base(context)
    {
    }

    public override string DisplayName { get; } = "Update Modules";

    protected override void DrawOverride(ref bool isGameViewFocused)
    {
        Context.Game.GameLogic.DrawUpdateModulesDiagnosticTable();
    }
}
