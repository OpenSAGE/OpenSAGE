using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;

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
            { "Texture", (parser, x) => x.Textures.Add(parser.ParseTextureReference()) }
        };

        public List<LazyAssetReference<TextureAsset>> Textures { get; } = new List<LazyAssetReference<TextureAsset>>();
    }
}
