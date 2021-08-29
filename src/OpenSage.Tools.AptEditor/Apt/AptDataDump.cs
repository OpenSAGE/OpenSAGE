using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenSage.FileFormats.Apt;
using System.Threading.Tasks;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptDataDump
    {
        public AptFile Apt { get; }
        public string Name { get; }
        public Dictionary<uint, string> GeometryData { get; }
        // TODO: Use texture instead of FileSystemEntry
        public int[] Textures { get; }

        public AptDataDump(AptFile file)
        {
            // TODO of course
            Apt = file;
            Name = file.MovieName;

            GeometryData = file.GeometryMap.ToDictionary(t => t.Key, t => t.Value.RawText);
            
            var textures = file.ImageMap.Mapping.Values
                .Select(m => m.TextureId)
                .Distinct();
            Textures = textures.ToArray();
            
        }

        public async Task WriteTo(DirectoryInfo target)
        {
            target.Create();
            // TODO: use a true XML serializer
            using var xml = new StreamWriter(File.Create(Path.Combine(target.FullName, $"{Name}.xml")));

            // head
            await xml.WriteLineAsync("<?xml version='1.0' encoding='utf-8'?>");
            await xml.WriteLineAsync("<AssetDeclaration xmlns=\"uri:ea.com:eala:asset\">");

            var getter = new StandardStreamGetter(target.FullName, Name);

            // apt
            Apt.Write(getter);
            var list = new[] { "Apt", "Const", "Dat" };
            foreach (var type in list)
            {
                var extension = type.ToLowerInvariant();
                var fileName = $"{Name}.{extension}";
                await xml.WriteLineAsync($"    <Apt{type}Data id=\"{Name}_{extension}\" File=\"{fileName}\" />");
            }

            // geometry
            var geometryDirectory = target.CreateSubdirectory($"{Name}_geometry");
            foreach (var (id, data) in GeometryData)
            {
                var ruName = Path.Combine(geometryDirectory.Name, $"{id}.ru");
                await File.WriteAllTextAsync(Path.Combine(target.FullName, ruName), data);
                await xml.WriteLineAsync($"    <AptGeometryData id=\"{Name}_{id}\" File=\"{ruName}\" AptID=\"{id}\" />");
            }

            // textures
            foreach (var texId in Textures)
            {
                string filePath = $"art\\Textures\\apt_{Name}_{texId}.tga";
                var directory = Path.GetDirectoryName(filePath);
                if (directory is string)
                    target.CreateSubdirectory(directory);

                using (var memory = new MemoryStream())
                {
                    using (var input = getter.GetTextureStream2((uint) texId, out var _, FileMode.Create))
                        await input.CopyToAsync(memory);
                    using var output = File.OpenWrite(Path.Combine(target.FullName, filePath));
                    memory.Seek(0, SeekOrigin.Begin);
                    await memory.CopyToAsync(output);
                }
                const string options = "OutputFormat=\"A8R8G8B8\" GenerateMipMaps=\"false\" AllowAutomaticResize=\"false\"";
                await xml.WriteLineAsync($"    <Texture id=\"{texId}\" File=\"{filePath}\" {options} />");
            }

            xml.WriteLine("</AssetDeclaration>");
        }

    }

}
