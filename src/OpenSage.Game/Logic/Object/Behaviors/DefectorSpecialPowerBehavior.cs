using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// When used in junction with the SPECIAL_DEFECTOR special power, the unit will defect to 
    /// your side.
    /// </summary>
    public sealed class DefectorSpecialPowerBehavior : ObjectBehavior
    {
        internal static DefectorSpecialPowerBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DefectorSpecialPowerBehavior> FieldParseTable = new IniParseTable<DefectorSpecialPowerBehavior>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
    }
}
