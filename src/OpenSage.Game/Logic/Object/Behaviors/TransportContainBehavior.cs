using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TransportContainBehavior : ObjectBehavior
    {
        internal static TransportContainBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransportContainBehavior> FieldParseTable = new IniParseTable<TransportContainBehavior>
        {
            { "PassengersAllowedToFire", (parser, x) => x.PassengersAllowedToFire = parser.ParseBoolean() },
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "DoorOpenTime", (parser, x) => x.DoorOpenTime = parser.ParseInteger() },
            { "ScatterNearbyOnExit", (parser, x) => x.ScatterNearbyOnExit = parser.ParseBoolean() }
        };

        public bool PassengersAllowedToFire { get; private set; }
        public int Slots { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public int DoorOpenTime { get; private set; }
        public bool ScatterNearbyOnExit { get; private set; }
    }
}
