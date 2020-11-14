using System.IO;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Writer;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptDataDump
    {
        public byte[] AptData { get; }
        public byte[] ConstantData { get; }
        public byte[] ImageMapData { get; }
        // TODO: public Dictionary<string, byte[]> GeometryData;
        public AptDataDump(AptFile file)
        {
            var aptData = AptDataWriter.Write(file.Movie);
            AptData = aptData.Data;
            ConstantData = ConstantDataWriter.Write(aptData.EntryOffset, file.Constants);
            ImageMapData = ImageMapWriter.Write(file.ImageMap);
        }

        public void WriteTo(DirectoryInfo target)
        {
            target.Create();
            var list = new[]
            {
                (AptData, ".apt"),
                (ConstantData, ".const"),
                (ImageMapData, ".dat")
            };
            foreach (var (data, extension) in list)
            {
                var fileName = Path.Combine(target.FullName, $"{target.Name}{extension}");
                File.WriteAllBytes(fileName, data);
            }
        }
    }

}
