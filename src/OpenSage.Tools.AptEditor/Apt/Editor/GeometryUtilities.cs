using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Apt;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    internal class GeometryUtilities
    {
        public IEnumerable<KeyValuePair<int, string>> Data =>
            Geometries.Select(kv => new KeyValuePair<int, string>((int) kv.Key, kv.Value.RawText));
        public AptEditManager Manager { get; }
        private Dictionary<uint, Geometry> Geometries => Manager.AptFile.GeometryMap;

        public GeometryUtilities(AptEditManager manager)
        {
            Manager = manager;
        }

        public void AddGeometry(string commands)
        {
            var max = Geometries.Any() ? Geometries.Keys.Max() : default;
            var key = max + 1;
            var newGeometry = GetGeometry(commands);
            var edit = new EditAction(() => Geometries.Add(key, newGeometry),
                                      () => Geometries.Remove(key));
            Manager.Edit(edit);
        }

        public void RemoveGeometry(int key)
        {
            var uKey = (uint) key;
            var oldGeometry = Geometries[uKey];
            var edit = new EditAction(() => Geometries.Remove(uKey),
                                      () => Geometries.Add(uKey, oldGeometry));
            Manager.Edit(edit);
        }

        public void UpdateGeometry(string editId, int key, string commands)
        {
            var uKey = (uint) key;
            var oldGeometry = Geometries[uKey];
            var newGeometry = GetGeometry(commands);
            var edit = new MergeableEdit(TimeSpan.FromSeconds(1.5),
                                         editId,
                                         () => Geometries[uKey] = newGeometry,
                                         () => Geometries[uKey] = oldGeometry,
                                         "Edit Geometry");
            Manager.Edit(edit);
        }

        private Geometry GetGeometry(string commands)
        {
            using var reader = new StringReader(commands);
            Geometry? geometry;
            try
            {
                geometry = Geometry.Parse(Manager.AptFile, reader);
            }
            catch (Exception e)
            when (e is InvalidOperationException or
                       IndexOutOfRangeException or
                       InvalidDataException or
                       OverflowException or
                       FormatException)
            {
                throw new AptEditorException(ErrorType.FailedToParseGeometry, e);
            }

            foreach (var entry in geometry.Entries)
            {
                switch (entry)
                {
                    case GeometryLines:
                    case GeometrySolidTriangles:
                        break;
                    case GeometryTexturedTriangles tt:
                        if (!Manager.AptFile.ImageMap.Mapping.ContainsKey(tt.Image))
                        {
                            throw new AptEditorException(ErrorType.InvalidTextureIdInGeometry, $"{tt.Image}");
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return geometry;
        }
    }
}
