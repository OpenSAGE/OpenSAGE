using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TransportContain : ObjectBehavior
    {
        internal static TransportContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TransportContain> FieldParseTable = new IniParseTable<TransportContain>
        {
            { "PassengersAllowedToFire", (parser, x) => x.PassengersAllowedToFire = parser.ParseBoolean() },
            { "Slots", (parser, x) => x.Slots = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
            { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
            { "GoAggressiveOnExit", (parser, x) => x.GoAggressiveOnExit = parser.ParseBoolean() },
            { "DoorOpenTime", (parser, x) => x.DoorOpenTime = parser.ParseInteger() },
            { "ScatterNearbyOnExit", (parser, x) => x.ScatterNearbyOnExit = parser.ParseBoolean() }
        };

        public bool PassengersAllowedToFire { get; private set; }
        public int Slots { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public int ExitDelay { get; private set; }
        public int NumberOfExitPaths { get; private set; }
        public bool GoAggressiveOnExit { get; private set; }
        public int DoorOpenTime { get; private set; }
        public bool ScatterNearbyOnExit { get; private set; }
    }
}
