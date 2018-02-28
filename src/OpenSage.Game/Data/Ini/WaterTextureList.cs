using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WaterTextureList
    {
        internal static WaterTextureList Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<WaterTextureList> FieldParseTable = new IniParseTable<WaterTextureList>
        {
            { "Texture", (parser, x) => x.Textures.Add(parser.ParseFileName()) }
        };

        public string Name { get; private set; }

        public List<string> Textures { get; private set; }
    }
}
