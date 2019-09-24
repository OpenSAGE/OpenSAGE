using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Terrain
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WaterTextureList : BaseAsset
    {
        internal static WaterTextureList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("WaterTextureList", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<WaterTextureList> FieldParseTable = new IniParseTable<WaterTextureList>
        {
            { "Texture", (parser, x) => x.Textures.Add(parser.ParseFileName()) }
        };

        public List<string> Textures { get; } = new List<string>();
    }
}
