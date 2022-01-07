using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.FileFormats.Apt
{
    public sealed class Import : IMemoryStorage
    {
        public string Movie { get; private set; }
        public string Name { get; private set; }
        public uint Character { get; private set; }
        public uint Pointer { get; private set; }

        public static Import Parse(BinaryReader reader)
        {
            var import = new Import();
            import.Movie = reader.ReadStringAtOffset();
            import.Name = reader.ReadStringAtOffset();
            import.Character = reader.ReadUInt32();
            import.Pointer = reader.ReadUInt32();

            return import;
        }

        public void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.WriteStringAtOffset(Movie, pool);
            writer.WriteStringAtOffset(Name, pool);
            writer.Write(Character);
            writer.Write(Pointer); 
        }
    }
}
