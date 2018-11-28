using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LevelGrantSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new LevelGrantSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LevelGrantSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<LevelGrantSpecialPowerModuleData>
            {
                { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
                { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() },
                { "Experience", (parser, x) => x.Experience = parser.ParseInteger() },
                { "RadiusEffect", (parser, x) => x.RadiusEffect = parser.ParseInteger() },
                { "AcceptanceFilter", (parser, x) => x.AcceptanceFilter = ObjectFilter.Parse(parser) },
                { "LevelFX", (parser, x) => x.LevelFX = parser.ParseAssetReference() },
            });

        public int UnpackingVariation { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }
        public int Experience { get; private set; }
        public int RadiusEffect { get; private set; }
        public ObjectFilter AcceptanceFilter { get; private set; }
        public string LevelFX { get; private set; }
    }
}
