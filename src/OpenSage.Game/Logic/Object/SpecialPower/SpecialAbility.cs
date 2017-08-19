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
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public bool UpdateModuleStartsAttack { get; private set; }
        public string InitiateSound { get; private set; }
    }
}
