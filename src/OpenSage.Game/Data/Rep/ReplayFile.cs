using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenSage.IO;

namespace OpenSage.Data.Rep
{
    public sealed class ReplayFile
    {
        public ReplayHeader Header { get; private set; }
        public IReadOnlyList<ReplayChunk> Chunks { get; private set; }

        public static ReplayFile FromFileSystemEntry(FileSystemEntry entry, bool onlyHeader = false)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.Unicode, true))
            {
                var result = new ReplayFile
                {
                    Header = ReplayHeader.Parse(reader)
                };

                if (onlyHeader)
                {
                    return result;
                }

                var chunks = new List<ReplayChunk>();
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    chunks.Add(ReplayChunk.Parse(reader));
                }
                result.Chunks = chunks;

                if (result.Header.NumTimecodes != chunks[chunks.Count - 1].Header.Timecode)
                {
                    throw new InvalidDataException();
                }

                return result;
            }
        }
    }
}
