using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of StartDive within UnitSpecificSounds section of the object.
    /// </summary>
    public sealed class DeliverPayloadAIUpdate : ObjectBehavior
    {
        internal static DeliverPayloadAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DeliverPayloadAIUpdate> FieldParseTable = new IniParseTable<DeliverPayloadAIUpdate>
        {
            { "DoorDelay", (parser, x) => x.DoorDelay = parser.ParseInteger() },
            { "MaxAttempts", (parser, x) => x.MaxAttempts = parser.ParseInteger() },
            { "DropOffset", (parser, x) => x.DropOffset = Coord3D.Parse(parser) },
            { "DropDelay", (parser, x) => x.DropDelay = parser.ParseInteger() },
            { "PutInContainer", (parser, x) => x.PutInContainer = parser.ParseAssetReference() },
            { "DeliveryDistance", (parser, x) => x.DeliveryDistance = parser.ParseInteger() }
        };

        public int DoorDelay { get; private set; }
        public int MaxAttempts { get; private set; }
        public Coord3D DropOffset { get; private set; }
        public int DropDelay { get; private set; }
        public string PutInContainer { get; private set; }
        public int DeliveryDistance { get; private set; }
    }
}
