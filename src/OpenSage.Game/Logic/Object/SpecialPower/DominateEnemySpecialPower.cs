using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DominateEnemySpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new DominateEnemySpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DominateEnemySpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<DominateEnemySpecialPowerModuleData>
            {
                { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
                { "DominateRadius", (parser, x) => x.DominateRadius = parser.ParseInteger() },
                { "DominatedFX", (parser, x) => x.DominatedFX = parser.ParseAssetReference() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
                { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() },
                { "PermanentlyConvert", (parser, x) => x.PermanentlyConvert = parser.ParseBoolean() },
                { "TriggerSound", (parser, x) => x.TriggerSound = parser.ParseAssetReference() }
            });

        public int UnpackingVariation { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int DominateRadius { get; private set; }
        public string DominatedFX { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool PermanentlyConvert { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string TriggerSound { get; private set; }
    }
}
