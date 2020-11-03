using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt.Characters;
using OpenSage.Tools.AptEditor.Apt;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    public sealed class CharacterUtilities
    {
        public IReadOnlyDictionary<int, string> CharacterNames { get { return _characterNames; } }
        private AptObjectsManager _manager;
        private Dictionary<int, string> _characterNames; // will be useful if user can assign arbitrary identifier name to character

        public CharacterUtilities()
        {
            Reset(null);
        }

        public void Reset(AptObjectsManager manager)
        {
            if(_manager == manager)
            {
                return;
            }

            if(manager == null)
            {
                _manager = null;
                _characterNames = null;
                return;
            }

            _manager = manager;
            _characterNames = new Dictionary<int, string>();
            for (var i = 0; i < _manager.AptFile.Movie.Characters.Count; ++i)
            {
                _characterNames.Add(i, $"#{i} {_manager.AptFile.Movie.Characters[i].GetType().Name}");
            }
        }

        public int NewCharacter(Character character, string name)
        {
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
            _manager.Edit(editAction);
            return characterId.Value;
        }

        public List<int> GetActiveCharacters()
        {
            // TODO: Handle Imported Characters
            return _manager.AptFile.Movie.Characters.Select((character, index) => index).ToList();
        }

        public string GetCharacterDescName(int index)
        {
            return _characterNames[index];
        }

        public List<(int, string, string)> GetActiveCharactersDescription()
        {
            System.Func<int, string> getCharacterDescName = (index) => _characterNames[index];
            return _manager.AptFile.Movie.Characters
                    .Select((character, index) => (index, character.GetType().Name, _characterNames[index])).ToList();
        }

        public Character GetCharacterByIndex(int index)
        {
            return _manager.AptFile.Movie.Characters[index];
        }
    }
}