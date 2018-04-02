using ImGuiNET;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;

namespace OpenSage.Viewer.UI
{
    internal sealed class MainForm
    {
        public MainForm()
        {
            var installations = GameInstallation.FindAll(GameDefinition.All);
        }

        public void Draw()
        {
            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("Installation"))
            {
                if (ImGui.MenuItem("About", "Ctrl-Alt-A", false, true))
                {

                }
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();

            ImGui.BeginWindow("Test");

            ImGui.Text("Hello,");
            ImGui.Text("World!");

            ImGui.EndWindow();
        }
    }
}
