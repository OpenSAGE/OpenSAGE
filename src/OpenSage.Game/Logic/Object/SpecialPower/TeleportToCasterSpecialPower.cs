using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class TeleportToCasterSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new TeleportToCasterSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<TeleportToCasterSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<TeleportToCasterSpecialPowerModuleData>
            {
                { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
                { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLOS = parser.ParseBoolean() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "TargetFX", (parser, x) => x.TargetFX = parser.ParseAssetReference() },
                { "MinDestinationRadius", (parser, x) => x.MinDestinationRadius = parser.ParseInteger() },
                { "MaxDestinationRadius", (parser, x) => x.MaxDestinationRadius = parser.ParseInteger() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
                { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() },
            });

        public int UnpackingVariation { get; private set; }
        public float StartAbilityRange { get; private set; }
        public bool ApproachRequiresLOS { get; private set; }
        public int Radius { get; private set; }
        public string TargetFX { get; private set; }
        public int MinDestinationRadius { get; private set; }
        public int MaxDestinationRadius { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }
    }
}
