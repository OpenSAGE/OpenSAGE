using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class CharacterList : IWidget
    {
        public uint Id { get; set; }
        private CharacterUtilities? _utilities;
        private int _lastSelectedCharacter;
        public CharacterList()
        {
            _utilities = null;
            _lastSelectedCharacter = -1;
        }
        public void Draw(AptSceneManager manager)
        {
            if(_utilities?.Manager != manager.AptManager)
            {
                _utilities = manager.AptManager is null
                    ? null
                    : new CharacterUtilities(manager.AptManager);
            }
            if(ImGui.Begin("Characters"))
            {
                ImGui.Text("Characters");
                ImGui.NewLine();

                ImGui.SameLine();
                if(ImGui.Button("New"))
                {
                    NewCharacter();
                }
                ImGui.SameLine();
                if(ImGui.Button("Delete"))
                {
                    DeleteCharacter();
                }
                ImGui.NewLine();

                if(_utilities != null)
                {
                    ListCharacterDescription(manager, ref _lastSelectedCharacter, _utilities);
                }
            }
            ImGui.End();
        }

        private static void ListCharacterDescription(AptSceneManager manager,
                                                     ref int lastSelectedCharacter,
                                                     CharacterUtilities utilities)
        {
            foreach (var (index, typeName, descName) in utilities.GetActiveCharactersDescription())
            {
                Vector4 indexColor = new Vector4(0, 1, 1, 1);
                Vector4 typeColor = new Vector4(0, 0.8f, 0.2f, 1);
                ImGui.SameLine(0, 5);
                ImGui.TextColored(indexColor, index.ToString());
                ImGui.SameLine(35, 5);
                ImGui.TextColored(typeColor, typeName);
                ImGui.SameLine(100, 5);

                bool wasSelected = (lastSelectedCharacter == index);
                bool selected = wasSelected;
                ImGui.Selectable(descName, ref selected);
                if (selected)
                {
                    lastSelectedCharacter = index;
                    var selectedCharacter = utilities.GetCharacterByIndex(index);
                    if (manager.CurrentCharacter != selectedCharacter)
                    {
                        System.Console.WriteLine($"Setting new character {index} {selectedCharacter.GetType().Name}");
                        manager.SetCharacter(selectedCharacter);
                    }

                }
                else if (wasSelected)
                {
                    lastSelectedCharacter = -1;
                }
                ImGui.NewLine();
            }
        }

        public void NewCharacter()
        {
            var action = new NonSymmetricEditAction<int>();
            //action.
        }

        public void DeleteCharacter()
        {

        }

    }
}
