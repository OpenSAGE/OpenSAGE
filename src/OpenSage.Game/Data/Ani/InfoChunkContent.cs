using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Ani
{
    public sealed class InfoChunkContent : RiffChunkContent
    {
        public string Value { get; private set; }

        internal static InfoChunkContent Parse(BinaryReader reader)
        {
            return new InfoChunkContent
            {
                Value = reader.ReadNullTerminatedString()
            };
        }
    }
}
