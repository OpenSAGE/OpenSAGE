using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpecialAbilityModuleData : SpecialPowerModuleData
    {
        internal static new SpecialAbilityModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpecialAbilityModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SpecialAbilityModuleData>
            {
                { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
            });

        public string InitiateSound { get; private set; }
    }
}
