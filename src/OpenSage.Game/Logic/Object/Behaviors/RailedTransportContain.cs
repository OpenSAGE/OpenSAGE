using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used on TRANSPORT KindOfs that follow a specific pre-defined waypoint path in a scripted 
    /// manner.
    /// </summary>
    public sealed class RailedTransportContain : ObjectBehavior
    {
        internal static RailedTransportContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RailedTransportContain> FieldParseTable = new IniParseTable<RailedTransportContain>
        {
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "DoorOpenTime", (parser, x) => x.DoorOpenTime = parser.ParseInteger() },
            { "ScatterNearbyOnExit", (parser, x) => x.ScatterNearbyOnExit = parser.ParseBoolean() }
        };

        public int Slots { get; private set; }
        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public int DoorOpenTime { get; private set; }
        public bool ScatterNearbyOnExit { get; private set; }
    }
}
