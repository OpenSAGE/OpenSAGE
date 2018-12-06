using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class CurseSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new CurseSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CurseSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CurseSpecialPowerModuleData>
            {
                { "CursePercentage", (parser, x) => x.CursePercentage = parser.ParsePercentage() },
                { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
                { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() },
                { "CursedFX", (parser, x) => x.CursedFX = parser.ParseAssetReference() },
            });

        public float CursePercentage { get; private set; }
        public int UnpackingVariation { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }
        public string CursedFX { get; private set; }
    }
}
