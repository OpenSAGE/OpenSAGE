using System.Numerics;
using ImGuiNET;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal abstract class DiagnosticView : DisposableBase
    {
        private bool _isVisible;

        protected DiagnosticViewContext Context { get; }
        protected Game Game { get; }
        protected ImGuiRenderer ImGuiRenderer { get; }

        public string Name => $"DiagnosticView {DisplayName}";

        public abstract string DisplayName { get; }
        public virtual Vector2 DefaultSize { get; } = new Vector2(400, 300);
        public virtual bool Closable { get; } = true;

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        protected DiagnosticView(DiagnosticViewContext context)
        {
            Context = context;
            Game = context.Game;
            ImGuiRenderer = context.ImGuiRenderer;
        }

        public void Draw(ref bool isGameViewFocused)
        {
            if (_isVisible)
            {
                ImGui.SetNextWindowSize(DefaultSize, ImGuiCond.FirstUseEver);

                if (Closable)
                {
                    if (ImGui.Begin($"{DisplayName}##{Name}", ref _isVisible))
                    {
                        DrawOverride(ref isGameViewFocused);
                    }
                    ImGui.End();
                }
                else
                {
                    ImGui.Begin(DisplayName);
                    DrawOverride(ref isGameViewFocused);
                    ImGui.End();
                }
            }
        }

        protected abstract void DrawOverride(ref bool isGameViewFocused);
    }
}
