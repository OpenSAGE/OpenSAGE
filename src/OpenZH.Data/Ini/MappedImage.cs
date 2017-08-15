using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class MappedImage
    {
        internal static MappedImage Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MappedImage> FieldParseTable = new IniParseTable<MappedImage>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "TextureWidth", (parser, x) => x.TextureWidth = parser.ParseInteger() },
            { "TextureHeight", (parser, x) => x.TextureHeight = parser.ParseInteger() },
            { "Coords", (parser, x) => x.Coords = MappedImageCoords.Parse(parser) },
            { "Status", (parser, x) => x.Status = parser.ParseEnum<MappedImageStatus>() },
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public int TextureWidth { get; private set; }
        public int TextureHeight { get; private set; }
        public MappedImageCoords Coords { get; private set; }
        public MappedImageStatus Status { get; private set; }
    }

    public struct MappedImageCoords
    {
        internal static MappedImageCoords Parse(IniParser parser)
        {
            return new MappedImageCoords
            {
                Left = parser.ParseAttributeInteger("Left"),
                Top = parser.ParseAttributeInteger("Top"),
                Right = parser.ParseAttributeInteger("Right"),
                Bottom = parser.ParseAttributeInteger("Bottom")
            };
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public enum MappedImageStatus
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("ROTATED_90_CLOCKWISE")]
        Rotated90Clockwise
    }
}
