using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ActivateModuleSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new ActivateModuleSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ActivateModuleSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<ActivateModuleSpecialPowerModuleData>
            {
                { "TriggerSpecialPower", (parser, x) => x.TriggerSpecialPower = parser.ParseAssetReference() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
                { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseInteger() },
                { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() },
                { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
                { "EffectRange", (parser, x) => x.EffectRange = parser.ParseFloat() }
            });

        public string TriggerSpecialPower { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PackTime { get; private set; }
        public int StartAbilityRange { get; private set; }
        public bool MustFinishAbility { get; private set; }
        public int UnpackingVariation { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public float EffectRange { get; private set; }
    }
}
