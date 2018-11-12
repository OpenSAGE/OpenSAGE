using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldMapInfo
    {
        internal static LivingWorldMapInfo Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldMapInfo> FieldParseTable = new IniParseTable<LivingWorldMapInfo>
        {
            //{ "Armor", (parser, x) => x.Values.Add(ArmorValue.Parse(parser)) }
        };

        public string Name { get; private set; }
    }
}
