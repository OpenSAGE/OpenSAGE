using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class AptButtonTooltipMap
    {
        internal static AptButtonTooltipMap Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AptButtonTooltipMap> FieldParseTable = new IniParseTable<AptButtonTooltipMap>
        {
            { "ButtonMap", (parser, x) => x.ButtonMaps.Add(parser.ParseAssetReference()) },
        };

        public List<string> ButtonMaps { get; } = new List<string>();
    }
}
