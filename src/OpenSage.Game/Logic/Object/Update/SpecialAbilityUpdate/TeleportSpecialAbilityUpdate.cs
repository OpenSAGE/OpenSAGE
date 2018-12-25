using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class TeleportSpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static TeleportSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TeleportSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<TeleportSpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLos = parser.ParseBoolean() },
            { "BusyForDuration", (parser, x) => x.BusyForDuration = parser.ParseInteger() },
            { "DestinationWeaponName", (parser, x) => x.DestinationWeaponName = parser.ParseString() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "SourceWeaponName", (parser, x) => x.SourceWeaponName = parser.ParseAssetReference() },
            { "MaxDistance", (parser, x) => x.MaxDistance = parser.ParseFloat() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackingVariation { get; private set; }
        public int UnpackTime { get; private set; }
        public int PackTime { get; private set; }
        public bool ApproachRequiresLos { get; private set; }
        public int BusyForDuration { get; private set; }
        public string DestinationWeaponName { get; private set; }
        public int PreparationTime { get; private set; }
        public string SourceWeaponName { get; private set; }
        public float MaxDistance { get; private set; }
    }
}
