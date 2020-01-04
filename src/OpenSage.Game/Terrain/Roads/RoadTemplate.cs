using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;

namespace OpenSage.Terrain.Roads
{
    public sealed class RoadTemplate : BaseAsset
    {
        internal static RoadTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("RoadTemplate", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<RoadTemplate> FieldParseTable = new IniParseTable<RoadTemplate>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseTextureReference() },
            { "RoadWidth", (parser, x) => x.RoadWidth = parser.ParseFloat() },
            { "RoadWidthInTexture", (parser, x) => x.RoadWidthInTexture = parser.ParseFloat() }
        };

        public LazyAssetReference<TextureAsset> Texture { get; private set; }
        public float RoadWidth { get; private set; }
        public float RoadWidthInTexture { get; private set; }
    }
}
