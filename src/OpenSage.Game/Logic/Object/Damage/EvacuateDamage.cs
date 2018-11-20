using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class EvacuateDamageModuleData : DamageModuleData
    {
        internal static EvacuateDamageModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EvacuateDamageModuleData> FieldParseTable = new IniParseTable<EvacuateDamageModuleData>
        {
            { "WeaponThatCausesEvacuation", (parser, x) => x.WeaponThatCausesEvacuation = parser.ParseString() }
        };

        public string WeaponThatCausesEvacuation { get; private set; }
    }
}
