using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Terrain
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WaterTextureList
    {
        internal static WaterTextureList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<WaterTextureList> FieldParseTable = new IniParseTable<WaterTextureList>
        {
            { "Texture", (parser, x) => x.Textures.Add(parser.ParseFileName()) }
        };

        public string Name { get; private set; }

        public List<string> Textures { get; } = new List<string>();
    }
}
