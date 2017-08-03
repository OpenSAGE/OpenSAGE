using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Ani
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
