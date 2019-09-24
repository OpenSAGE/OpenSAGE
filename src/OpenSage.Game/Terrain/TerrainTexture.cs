using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Terrain
{
    public sealed class TerrainTexture : BaseAsset
    {
        internal static TerrainTexture Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("TerrainTexture", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<TerrainTexture> FieldParseTable = new IniParseTable<TerrainTexture>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseAssetReference() },
            { "BlendEdges", (parser, x) => x.BlendEdges = parser.ParseBoolean() },
            { "Class", (parser, x) => x.Class = parser.ParseString() },
            { "RestrictConstruction", (parser, x) => x.RestrictConstruction = parser.ParseBoolean() },
            { "TerrainObject", (parser, x) => x.TerrainObjects.Add(TerrainObject.Parse(parser)) },
        };

        public string Texture { get; private set; }
        public bool BlendEdges { get; private set; }
        public string Class { get; private set; }
        public bool RestrictConstruction { get; private set; }
        public List<TerrainObject> TerrainObjects { get; } = new List<TerrainObject>();
    }

    public struct TerrainObject
    {
        internal static TerrainObject Parse(IniParser parser)
        {
            return new TerrainObject
            {
                Name = parser.ParseAssetReference(),
                Unknown = parser.ParseInteger()
            };
        }

        public string Name { get; private set; }
        public int Unknown { get; private set; }
    }
}
