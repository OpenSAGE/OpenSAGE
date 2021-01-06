using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    internal sealed class CharacterUtilities
    {
        public sealed class Description
        {
            public Import? Import { get; }
            public string? ExportedName { get; }
            public int Index { get; }
            public string Type { get; }
            public string Name { get; }
            public Vector4? ShapeBounds { get; }
            public int? ShapeGeometry { get; }

            public Description(Movie movie, int index, string name)
            {
                var character = movie.Characters[index];
                Import = movie.Imports.FirstOrDefault(i => i.Character == index);
                ExportedName = movie.Exports.FirstOrDefault(e => e.Character == index)?.Name;
                Index = index;
                Type = character.GetType().Name;
                Name = name;
                if (character is Shape shape)
                {
                    ShapeBounds = shape.Bounds;
                    ShapeGeometry = (int) shape.Geometry;
                }
            }
        }

        private readonly Dictionary<int, string> _characterNames = new Dictionary<int, string>(); // will be useful if user can assign arbitrary identifier name to character
        private Description[]? _cachedDescriptions;

        public IReadOnlyDictionary<int, string> CharacterNames => _characterNames;
        public AptEditManager Manager { get; }
        private Movie Movie => Manager.AptFile.Movie;

        public CharacterUtilities(AptEditManager manager)
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

        public bool IsGeometryIdValid(int geometryId)
        {
            return geometryId >= 0 && Manager.AptFile.GeometryMap.ContainsKey((uint) geometryId);
        }

        public void SetShapeGeometry(int shapeIndex, int geometryId)
        {
            var shape = (Shape) GetCharacterByIndex(shapeIndex);
            var current = shape.Geometry;
            var edit = new EditAction(() => shape.Modify((uint) geometryId),
                                      () => shape.Modify(current),
                                      "Set Shape Geometry");
            edit.OnEdit += InvalidateCache;
            Manager.Edit(edit);
        }

        public void CreateShape(string? name)
        {
            var apt = Manager.AptFile;
            var geometry = Geometry.Parse(apt, TextReader.Null);
            var uKey = (uint) Movie.Characters.Count;
            apt.GeometryMap.TryAdd(uKey, geometry);
            void SetupGeometry()
            {
                apt.GeometryMap.TryAdd(uKey, geometry);
            }
            void UndoGeometry()
            {
                if (apt.GeometryMap.TryGetValue(uKey, out var stored) &&
                    stored == geometry &&
                    !stored.RawText.Any())
                {
                    apt.GeometryMap.Remove(uKey);
                }
            }
            CreateCharacter(name,
                            Shape.Create(Manager.AptFile, uKey),
                            SetupGeometry,
                            UndoGeometry);
        }

        public void CreateSprite(string? name)
        {
            CreateCharacter(name, Sprite.Create(Manager.AptFile));
        }

        private void CreateCharacter(string? name, Character created, Action? beforeEdit = null, Action? beforeUndo = null)
        {
            var type = created.GetType().Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"#{_characterNames.Count} {type}";
            }
            var characters = Movie.Characters;
            var expectedIndex = characters.Count;
            beforeEdit += () => characters.Add(created);
            beforeUndo += () => characters.RemoveAt(expectedIndex);
            var edit = new EditAction(beforeEdit, beforeUndo, $"Create {type}");
            edit.OnEdit += delegate { _characterNames[expectedIndex] = name; };
            edit.OnEdit += InvalidateCache;
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
            edit.Description = "Export";
            edit.OnEdit += InvalidateCache;
            Manager.Edit(edit);
        }

        public void CancelExport(int index)
        {
            var exports = Manager.AptFile.Movie.Exports;
            var exportIndex = exports.FindIndex(e => e.Character == index);
            var export = exports[exportIndex];
            var edit = new EditAction(() => exports.RemoveAt(exportIndex),
                                      () => exports.Insert(exportIndex, export),
                                      "Cancel Export");
            edit.OnEdit += InvalidateCache;
            Manager.Edit(edit);
        }

        private void InvalidateCache(object? sender, EventArgs e)
        {
            _cachedDescriptions = null;
        }
    }
}
