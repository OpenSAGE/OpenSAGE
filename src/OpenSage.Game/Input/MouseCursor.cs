using OpenSage.Data.Ini;
using System.Numerics;

namespace OpenSage.Input
{
    public sealed class MouseCursor
    {
        internal static MouseCursor Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MouseCursor> FieldParseTable = new IniParseTable<MouseCursor>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "Image", (parser, x) => x.Image = parser.ParseFileName() },
            { "Directions", (parser, x) => x.Directions = parser.ParseInteger() },
            { "HotSpot", (parser, x) => x.HotSpot = parser.ParseVector2() }
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public string Image { get; private set; }
        public int Directions { get; private set; }
        public Vector2 HotSpot { get; private set; }
    }
}
