using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Writer;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptDataDump
    {
        public string Name { get; }
        public byte[] AptData { get; }
        public byte[] ConstantData { get; }
        public byte[] ImageMapData { get; }
        public Dictionary<uint, string> GeometryData { get; }
        // TODO: textures

        public AptDataDump(AptFile file)
        {
            Name = file.MovieName;
            var aptData = AptDataWriter.Write(file.Movie);
            AptData = aptData.Data;
            ConstantData = ConstantDataWriter.Write(aptData.EntryOffset, file.Constants);
            ImageMapData = ImageMapWriter.Write(file.ImageMap);
            GeometryData = file.GeometryMap.ToDictionary(t => t.Key, t => t.Value.RawText);
        }

        public void WriteTo(DirectoryInfo target)
        {
            target.Create();
            // TODO: use a true XML serializer
            using var xml = new StreamWriter(File.OpenWrite(Path.Combine(target.FullName, $"{Name}.xml")));
            xml.WriteLine("<?xml version='1.0' encoding='utf-8'?>");
            xml.WriteLine("<AssetDeclaration xmlns=\"uri:ea.com:eala:asset\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance \">");
            var list = new[]
            {
                (AptData, "Apt"),
                (ConstantData, "Const"),
                (ImageMapData, "Dat")
            };
            foreach (var (data, type) in list)
            {
                var extension = type.ToLowerInvariant();
                var fileName = $"{Name}.{extension}";
                File.WriteAllBytes(Path.Combine(target.FullName, fileName), data);
                xml.WriteLine($"    <Apt{type}Data id=\"{Name}_{extension}\" File=\"{fileName}\" />");
            }

            var geometryDirectory = target.CreateSubdirectory($"{Name}_geometry");
            foreach (var (id, data) in GeometryData)
            {
                var ruName = Path.Combine(geometryDirectory.Name, $"{id}.ru");
                xml.WriteLine($"    <AptGeometryData id=\"{Name}_{id}\" File=\"{ruName}\" AptID=\"{id}\"/>");
                File.WriteAllText(Path.Combine(target.FullName, ruName), data);
            }

            xml.WriteLine("</AssetDeclaration>");
        }
    }

}
