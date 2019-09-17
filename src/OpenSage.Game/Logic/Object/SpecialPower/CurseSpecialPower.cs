using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

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
                { "TriggerModelCondition", (parser, x) => x.TriggerModelCondition = parser.ParseAttributeEnum<ModelConditionFlag>("ModelConditionState") },
                { "TriggerModelConditionDuration", (parser, x) => x.TriggerModelConditionDuration = parser.ParseInteger() },
                { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
                { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) }
            });

        public Percentage CursePercentage { get; private set; }
        public int UnpackingVariation { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }
        public string CursedFX { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public ModelConditionFlag TriggerModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TriggerModelConditionDuration { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int PackTime { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public AnimAndDuration CustomAnimAndDuration { get; private set; }
    }
}
