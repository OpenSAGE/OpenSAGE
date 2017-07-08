using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class PolygonTrigger
    {
        public string Name { get; private set; }

        public static PolygonTrigger Parse(BinaryReader reader)
        {
            var name = reader.ReadUInt16PrefixedAsciiString();

            return new PolygonTrigger
            {
                Name = name
            };
        }
    }
}
