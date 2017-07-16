using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenZH.Data.Wak
{
    /// <summary>
    /// Wave definition file.
    /// </summary>
    public sealed class WakFile
    {
        public WakEntry[] Entries { get; private set; }

        public static WakFile Parse(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                var entries = new List<WakEntry>();
                while (stream.Length - stream.Position > 20)
                {
                    entries.Add(WakEntry.Parse(reader));
                }

                var numEntries = reader.ReadUInt32();
                if (entries.Count != numEntries)
                {
                    throw new InvalidDataException();
                }

                if (stream.Position != stream.Length)
                {
                    throw new InvalidDataException();
                }

                return new WakFile
                {
                    Entries = entries.ToArray()
                };
            }
        }

        public void WriteTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream, Encoding.ASCII, true))
            {
                foreach (var entry in Entries)
                {
                    entry.WriteTo(writer);
                }

                writer.Write((uint) Entries.Length);
            }
        }
    }
}
