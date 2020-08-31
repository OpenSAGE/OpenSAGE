using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class InspectorView : DiagnosticView
    {
        private object _currentSelectedObject;
        private IInspectable _currentInspectable;

        public InspectorView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        public override string DisplayName { get; } = "Inspector";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Context.SelectedObject != _currentSelectedObject)
            {
                _currentInspectable =
                    (Context.SelectedObject as IInspectable)
                    ?? new DefaultInspectable(Context.SelectedObject, Context);

                _currentSelectedObject = Context.SelectedObject;
            }

            if (_currentInspectable != null)
            {
                ImGui.Text(_currentInspectable.Name);

                ImGui.Separator();

                ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.55f);

                _currentInspectable.DrawInspector();

                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.Text("Nothing selected");
            }
        }
    }
}
