using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpecialAbilityModuleData : SpecialPowerModuleData
    {
        internal static SpecialAbilityModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpecialAbilityModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SpecialAbilityModuleData>
            {
                { "UpdateModuleStartsAttack", (parser, x) => x.UpdateModuleStartsAttack = parser.ParseBoolean() },
                { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
            });

        public bool UpdateModuleStartsAttack { get; private set; }
        public string InitiateSound { get; private set; }
    }
}
