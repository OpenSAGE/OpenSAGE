using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyGainCreate : ObjectBehavior
    {
        internal static VeterancyGainCreate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<VeterancyGainCreate> FieldParseTable = new IniParseTable<VeterancyGainCreate>
        {
            { "StartingLevel", (parser, x) => x.StartingLevel = parser.ParseEnum<VeterancyLevel>() },
            { "ScienceRequired", (parser, x) => x.ScienceRequired = parser.ParseAssetReference() }
        };

        public VeterancyLevel StartingLevel { get; private set; }
        public string ScienceRequired { get; private set; }
    }
}
