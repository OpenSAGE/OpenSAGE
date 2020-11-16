using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    public sealed class CharacterUtilities
    {
        public sealed class Description
        {
            public Import? Import { get; }
            public string? ExportedName { get; }
            public int Index { get; }
            public string Type { get; }
            public string Name { get; }

            public Description(Movie movie, int index, string name)
            {
                Import = movie.Imports.FirstOrDefault(i => i.Character == index);
                ExportedName = movie.Exports.FirstOrDefault(e => e.Character == index)?.Name;
                Index = index;
                Type = movie.Characters[index].GetType().Name;
                Name = name;
            }
        }
        
        private readonly Dictionary<int, string> _characterNames = new Dictionary<int, string>(); // will be useful if user can assign arbitrary identifier name to character
        private Description[]? _cachedDescriptions;

        public IReadOnlyDictionary<int, string> CharacterNames => _characterNames;
        public AptObjectsManager Manager { get; }
        private Movie Movie => Manager.AptFile.Movie;

        public CharacterUtilities(AptObjectsManager manager)
        {
            Manager = manager;
            for (var i = 0; i < Movie.Characters.Count; ++i)
            {
                _characterNames.Add(i, $"#{i} {Movie.Characters[i].GetType().Name}");
            }
        }

        public IEnumerable<Description> GetActiveCharactersDescription()
        {
            if (_cachedDescriptions != null)
            {
                return _cachedDescriptions;
            }
            _cachedDescriptions = Movie.Characters
                .Select((character, index) => new Description(Movie, index, _characterNames[index]))
                .ToArray();
            return _cachedDescriptions;
        }

        public Character GetCharacterByIndex(int index)
        {
            return Movie.Characters[index];
        }

        public void CreateShape(string name, int geometryId)
        {
            CreateCharacter(name, () => Shape.Create(Manager.AptFile, (uint) geometryId));
        }

        public void CreateSprite(string name)
        {
            CreateCharacter(name, () => Sprite.Create(Manager.AptFile, new List<Frame>()));
        }

        private void CreateCharacter(string name, Action characterCreation)
        {
            var expectedIndex = Movie.Characters.Count;
            var edit = new EditAction(characterCreation,
                                      () => Movie.Characters.RemoveAt(expectedIndex));
            edit.OnEdit += delegate
            {
                _cachedDescriptions = null;
                _characterNames[expectedIndex] = name;
            };
            Manager.Edit(edit);
        }

        public void ExportCharacter(int index, string name)
        {
            var exports = Manager.AptFile.Movie.Exports;
            var exportIndex = exports.FindIndex(e => e.Character == index);
            var newExport = Export.Create(name, index);
            var edit = exportIndex == -1
                ? new EditAction<Export>(exports.Add, e => exports.Remove(e), newExport)
                : new EditAction<Export>(e => exports[exportIndex] = e, newExport);
            edit.OnEdit += delegate { _cachedDescriptions = null; };
            Manager.Edit(edit);
        }

        public void CancelExport(int index)
        {
            var exports = Manager.AptFile.Movie.Exports;
            var exportIndex = exports.FindIndex(e => e.Character == index);
            var export = exports[exportIndex];
            var edit = new EditAction(() => exports.RemoveAt(exportIndex),
                                      () => exports.Insert(exportIndex, export));
            edit.OnEdit += delegate { _cachedDescriptions = null; };
            Manager.Edit(edit);
        }
    }
}
