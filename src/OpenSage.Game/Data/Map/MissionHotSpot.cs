using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Ra3)]
    public sealed class MissionHotSpot
    {
        public string ID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }

        internal static MissionHotSpot Parse(BinaryReader reader)
        {
            return new MissionHotSpot
            {
                ID = reader.ReadUInt16PrefixedAsciiString(),
                Title = reader.ReadUInt16PrefixedAsciiString(),
                Description = reader.ReadUInt16PrefixedAsciiString()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteUInt16PrefixedAsciiString(ID);
            writer.WriteUInt16PrefixedAsciiString(Title);
            writer.WriteUInt16PrefixedAsciiString(Description);
        }
    }
}
