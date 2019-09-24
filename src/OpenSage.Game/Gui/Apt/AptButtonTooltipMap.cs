using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Gui.Apt
{
    public sealed class AptButtonTooltipMap : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, AptButtonTooltipMap value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<AptButtonTooltipMap> FieldParseTable = new IniParseTable<AptButtonTooltipMap>
        {
            { "ButtonMap", (parser, x) => x.ButtonMaps.Add(parser.ParseAssetReference()) },
        };

        public List<string> ButtonMaps { get; } = new List<string>();
    }
}
