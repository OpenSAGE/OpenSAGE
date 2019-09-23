using OpenSage.Content;
using OpenSage.Data.Ini;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class RoadTemplate
    {
        internal static RoadTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<RoadTemplate> FieldParseTable = new IniParseTable<RoadTemplate>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseTextureReference() },
            { "RoadWidth", (parser, x) => x.RoadWidth = parser.ParseFloat() },
            { "RoadWidthInTexture", (parser, x) => x.RoadWidthInTexture = parser.ParseFloat() }
        };

        public string Name { get; private set; }

        public LazyAssetReference<Texture> Texture { get; private set; }
        public float RoadWidth { get; private set; }
        public float RoadWidthInTexture { get; private set; }
    }
}
