using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpecialAbility : ObjectBehavior
    {
        internal static SpecialAbility Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialAbility> FieldParseTable = new IniParseTable<SpecialAbility>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "UpdateModuleStartsAttack", (parser, x) => x.UpdateModuleStartsAttack = parser.ParseBoolean() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public bool UpdateModuleStartsAttack { get; private set; }
    }
}
