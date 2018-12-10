using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldArmyIcon
    {
        internal static LivingWorldArmyIcon Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldArmyIcon> FieldParseTable = new IniParseTable<LivingWorldArmyIcon>
        {
            { "OnSelectedSound", (parser, x) => x.OnSelectedSound = parser.ParseAssetReference() },
            { "OnMoveSound", (parser, x) => x.OnMoveSound = parser.ParseAssetReference() },
            { "Object", (parser, x) => x.Objects.Add(Object.Parse(parser)) },
        };

        public string OnSelectedSound { get; private set; }
        public string OnMoveSound { get; private set; }
        public List<Object> Objects { get; } = new List<Object>();
    }

    public sealed class Object
    {
        internal static Object Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Object> FieldParseTable = new IniParseTable<Object>
        {
            { "Model", (parser, x) => x.Model = parser.ParseAssetReference() },
            { "ZOffset", (parser, x) => x.ZOffset = parser.ParseInteger() },
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },
            { "Clickable", (parser, x) => x.Clickable = parser.ParseBoolean() },
            { "Hidden", (parser, x) => x.Hidden = parser.ParseBoolean() },
            { "CanFade", (parser, x) => x.CanFade = parser.ParseBoolean() },
            { "OrientAngle", (parser, x) => x.OrientAngle = parser.ParseFloat() },
            { "Pickbox", (parser, x) => x.Pickbox = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }

        public string Model { get; private set; }
        public int ZOffset { get; private set; }
        public float Scale { get; private set; }
        public bool Clickable { get; private set; }
        public bool Hidden { get; private set; }
        public bool CanFade { get; private set; }
        public float OrientAngle { get; private set; }
        public string Pickbox { get; private set; }
    }
}
