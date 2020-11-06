using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt.Characters;
using OpenSage.Tools.AptEditor.Apt;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    public sealed class CharacterUtilities
    {
        public IReadOnlyDictionary<int, string> CharacterNames => _characterNames;
        public AptObjectsManager Manager { get; }
        private readonly Dictionary<int, string> _characterNames = new Dictionary<int, string>(); // will be useful if user can assign arbitrary identifier name to character

        public CharacterUtilities(AptObjectsManager manager)
        {
            Manager = manager;
            for (var i = 0; i < Manager.AptFile.Movie.Characters.Count; ++i)
            {
                _characterNames.Add(i, $"#{i} {Manager.AptFile.Movie.Characters[i].GetType().Name}");
            }
        }

        public int NewCharacter(Character character, string name)
        {
            throw new NotImplementedException();
            int? characterId = null;
            var editAction = new ListAddAction<Character>(character);
            editAction.Description = "Add New Character";
            editAction.FindList = (aptFile) =>
            {
                return aptFile.Movie.Characters;
            };
            editAction.BeforeDo = (aptFile, list) =>
            {
                if (list.Contains(character))
                {
                    throw new AptEditorException(ErrorType.CannotAddExistingCharacter);
                }
            };
            Manager.Edit(editAction);
            return characterId.Value;
        }

        public List<int> GetActiveCharacters()
        {
            // TODO: Handle Imported Characters
            return Manager.AptFile.Movie.Characters.Select((character, index) => index).ToList();
        }

        public string GetCharacterDescName(int index)
        {
            return _characterNames[index];
        }

        public List<(int, string, string)> GetActiveCharactersDescription()
        {
            Func<int, string> getCharacterDescName = (index) => _characterNames[index];
            return Manager.AptFile.Movie.Characters
                    .Select((character, index) => (index, character.GetType().Name, _characterNames[index])).ToList();
        }

        public Character GetCharacterByIndex(int index)
        {
            return Manager.AptFile.Movie.Characters[index];
        }
    }
}
