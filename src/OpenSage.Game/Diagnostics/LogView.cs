using System.Numerics;
using ImGuiNET;
using NLog;
using OpenSage.Core;

namespace OpenSage.Diagnostics
{
    internal sealed class LogView : DiagnosticView
    {
        public LogView(DiagnosticViewContext context) : base(context)
        {
        }

        public override string DisplayName => "Log View";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            var target = LogManager.Configuration.FindTargetByName("internal") as InternalLogger;

            ImGui.BeginChild("Log", Vector2.Zero, false);

            foreach (var msg in target.Messages)
            {
                var color = Vector4.One;

                if(msg.Level == LogLevel.Warn)
                {
                    color = new Vector4(1, 1, 0, 1);
                }
                else if (msg.Level >= LogLevel.Error)
                {
                    color = new Vector4(1, 0, 0, 1);
                }
                else if (msg.Level == LogLevel.Info)
                {
                    color = new Vector4(0, 0, 1, 1);
                }

                ImGui.TextColored(color, msg.Content);
            }

            ImGui.EndChild();
        }
    }
}
