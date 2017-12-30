using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class PostEffect
    {
        public string Name { get; private set; }

        // Only applicable to LookupTable?
        public float BlendFactor { get; private set; }
        public string LookupImage { get; private set; }

        internal static PostEffect Parse(BinaryReader reader)
        {
            return new PostEffect
            {
                Name = reader.ReadUInt16PrefixedAsciiString(),
                BlendFactor = reader.ReadSingle(),
                LookupImage = reader.ReadUInt16PrefixedAsciiString()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.Write(BlendFactor);
            writer.WriteUInt16PrefixedAsciiString(LookupImage);
        }
    }
}
