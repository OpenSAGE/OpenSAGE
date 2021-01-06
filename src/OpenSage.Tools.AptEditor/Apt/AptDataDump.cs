using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Writer;
using OpenSage.Data;
using System.Threading.Tasks;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptDataDump
    {
        public string Name { get; }
        public byte[] AptData { get; }
        public byte[] ConstantData { get; }
        public byte[] ImageMapData { get; }
        public Dictionary<uint, string> GeometryData { get; }
        // TODO: Use texture instead of FileSystemEntry
        public FileSystemEntry[] Textures { get; }

        public AptDataDump(AptFile file)
        {
            Name = file.MovieName;
            var aptData = AptDataWriter.Write(file.Movie);
            AptData = aptData.Data;
            ConstantData = ConstantDataWriter.Write(aptData.EntryOffset, file.Constants);
            ImageMapData = ImageMapWriter.Write(file.ImageMap);
            GeometryData = file.GeometryMap.ToDictionary(t => t.Key, t => t.Value.RawText);
            var textures = file.ImageMap.Mapping.Values
                .Select(m => m.TextureId)
                .Distinct()
                .Select(id => FindTexture(file.FileSystem, $"apt_{file.MovieName}_{id}.tga"));
            Textures = textures.ToArray();
        }

        public async Task WriteTo(DirectoryInfo target)
        {
            target.Create();
            // TODO: use a true XML serializer
            using var xml = new StreamWriter(File.Create(Path.Combine(target.FullName, $"{Name}.xml")));
            await xml.WriteLineAsync("<?xml version='1.0' encoding='utf-8'?>");
            await xml.WriteLineAsync("<AssetDeclaration xmlns=\"uri:ea.com:eala:asset\">");
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
                await File.WriteAllBytesAsync(Path.Combine(target.FullName, fileName), data);
                await xml.WriteLineAsync($"    <Apt{type}Data id=\"{Name}_{extension}\" File=\"{fileName}\" />");
            }

            var geometryDirectory = target.CreateSubdirectory($"{Name}_geometry");
            foreach (var (id, data) in GeometryData)
            {
                var ruName = Path.Combine(geometryDirectory.Name, $"{id}.ru");
                await File.WriteAllTextAsync(Path.Combine(target.FullName, ruName), data);
                await xml.WriteLineAsync($"    <AptGeometryData id=\"{Name}_{id}\" File=\"{ruName}\" AptID=\"{id}\" />");
            }

            foreach (var texture in Textures)
            {
                var directory = Path.GetDirectoryName(texture.FilePath);
                if (directory is string)
                {
                    target.CreateSubdirectory(directory);
                }
                using (var memory = new MemoryStream())
                {
                    using (var input = texture.Open())
                    {
                        await input.CopyToAsync(memory);
                    }
                    using var output = File.OpenWrite(Path.Combine(target.FullName, texture.FilePath));
                    memory.Seek(0, SeekOrigin.Begin);
                    await memory.CopyToAsync(output);
                }
                var id = Path.GetFileNameWithoutExtension(texture.FilePath);
                const string options = "OutputFormat=\"A8R8G8B8\" GenerateMipMaps=\"false\" AllowAutomaticResize=\"false\"";
                await xml.WriteLineAsync($"    <Texture id=\"{id}\" File=\"{texture.FilePath}\" {options} />");
            }

            xml.WriteLine("</AssetDeclaration>");
        }

        private static FileSystemEntry FindTexture(FileSystem fileSystem, string name)
        {
            foreach (var path in AptEditorDefinition.TexturePathResolver.GetPaths(name, string.Empty))
            {
                if (fileSystem.GetFile(path) is FileSystemEntry entry)
                {
                    return entry;
                }
            }
            throw new FileNotFoundException("Cannot find texture to export", name);
        }
    }

}
