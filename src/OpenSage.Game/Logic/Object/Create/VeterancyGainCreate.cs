using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyGainCreate : ObjectBehavior
    {
        internal static VeterancyGainCreate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<VeterancyGainCreate> FieldParseTable = new IniParseTable<VeterancyGainCreate>
        {
            { "StartingLevel", (parser, x) => x.StartingLevel = parser.ParseEnum<VeterancyLevel>() }
        };

        public VeterancyLevel StartingLevel { get; private set; }
    }
}
