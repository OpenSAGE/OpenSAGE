using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui
{
    public sealed class MappedImage : BaseAsset
    {
        internal static MappedImage Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("MappedImage", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<MappedImage> FieldParseTable = new IniParseTable<MappedImage>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseGuiTextureReference() },
            { "TextureWidth", (parser, x) => x.TextureWidth = parser.ParseInteger() },
            { "TextureHeight", (parser, x) => x.TextureHeight = parser.ParseInteger() },
            { "Coords", (parser, x) => x.Coords = ParseCoords(parser) },
            { "Status", (parser, x) => x.Status = parser.ParseEnum<MappedImageStatus>() },
        };

        public LazyAssetReference<GuiTextureAsset> Texture { get; private set; }

        public Size TextureDimensions { get; private set; }

        private int TextureWidth
        {
            set => TextureDimensions = new Size(value, TextureDimensions.Height);
        }

        private int TextureHeight
        {
            set => TextureDimensions = new Size(TextureDimensions.Width, value);
        }

        public Rectangle Coords { get; private set; }
        public MappedImageStatus Status { get; private set; }

        private static Rectangle ParseCoords(IniParser parser)
        {
            var left = parser.ParseAttributeInteger("Left");
            var top = parser.ParseAttributeInteger("Top");
            var right = parser.ParseAttributeInteger("Right");
            var bottom = parser.ParseAttributeInteger("Bottom");

            return new Rectangle(
                left,
                top,
                right - left,
                bottom - top);
        }
    }

    public enum MappedImageStatus
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("ROTATED_90_CLOCKWISE")]
        Rotated90Clockwise
    }
}
