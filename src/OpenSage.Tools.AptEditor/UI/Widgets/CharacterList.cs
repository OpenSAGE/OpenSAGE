using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Apt;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class CharacterList : IWidget
    {
        private CharacterUtilities? _utilities = null;
        private int _lastSelectedCharacter = -1;
        private string? _decidingExportName = null;

        public void Draw(AptSceneManager manager)
        {
            if (_utilities?.Manager != manager.AptManager)
            {
                _lastSelectedCharacter = -1;
                _decidingExportName = null;
                _utilities = manager.AptManager is null
                    ? null
                    : new CharacterUtilities(manager.AptManager);
            }
            if (ImGui.Begin("Characters"))
            {
                ImGui.Text("Characters");
                ImGui.NewLine();

                ImGui.SameLine();
                if (ImGui.Button("New"))
                {
                    NewCharacter();
                }
                ImGui.SameLine();
                if (ImGui.Button("Delete"))
                {
                    DeleteCharacter();
                }
                ImGui.NewLine();

                if (_utilities != null)
                {
                    ListCharacterDescription(manager);
                }
            }
            ImGui.End();
        }

        private void ListCharacterDescription(AptSceneManager manager)
        {
            var indexColor = new Vector4(0, 1, 1, 1);
            var typeColor = new Vector4(0, 0.8f, 0.2f, 1);
            var exportColor = new Vector4(1, 1, 0, 1);
            foreach (var desc in _utilities!.GetActiveCharactersDescription())
            {
                var wasSelected = (_lastSelectedCharacter == desc.Index);

                if (desc.Import is Import import)
                {
                    ImGui.TextColored(indexColor, "Imported");
                    ImGui.SameLine(0, 5);
                    ImGui.Text($"{import.Movie}.{import.Name}");
                }

                if (desc.ExportedName != null)
                {
                    ImGui.TextColored(exportColor, "Exported");
                    ImGui.SameLine(0, 5);
                    ImGui.Text(desc.ExportedName);
                }

                ImGui.TextColored(indexColor, desc.Index.ToString());
                ImGui.SameLine(0, 5);
                ImGui.TextColored(typeColor, desc.Type);
                ImGui.SameLine(0, 5);


                var selected = wasSelected;
                ImGui.Selectable(desc.Name, ref selected);
                if (selected)
                {
                    _lastSelectedCharacter = desc.Index;
                    if (!wasSelected)
                    {
                        _decidingExportName = null;
                    }
                    var selectedCharacter = _utilities.GetCharacterByIndex(desc.Index);
                    if (manager.CurrentCharacter != selectedCharacter)
                    {
                        System.Console.WriteLine($"Setting new character {desc.Index} {selectedCharacter.GetType().Name}");
                        manager.SetCharacter(selectedCharacter);
                    }

                    _decidingExportName ??= desc.ExportedName ?? desc.Name;
                    ImGui.InputText("Exported name", ref _decidingExportName, 64);
                    if (desc.ExportedName == null)
                    {
                        if (ImGui.Button("Export"))
                        {
                            _utilities.ExportCharacter(desc.Index, _decidingExportName);
                        }
                    }
                    else
                    {
                        if (ImGui.Button("Cancel export"))
                        {
                            _utilities.CancelExport(desc.Index);
                        }
                    }

                }

                ImGui.Separator();
            }
        }

        public void NewCharacter()
        {

            //action.
        }

        public void DeleteCharacter()
        {

        }

    }
}
