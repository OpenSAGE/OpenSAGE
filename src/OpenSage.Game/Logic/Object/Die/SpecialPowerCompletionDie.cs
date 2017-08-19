using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpecialPowerCompletionDie : ObjectBehavior
    {
        internal static SpecialPowerCompletionDie Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialPowerCompletionDie> FieldParseTable = new IniParseTable<SpecialPowerCompletionDie>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
    }
}
