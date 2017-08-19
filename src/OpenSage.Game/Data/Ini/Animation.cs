using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class Animation
    {
        internal static Animation Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Animation> FieldParseTable = new IniParseTable<Animation>
        {
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationDelay", (parser, x) => x.AnimationDelay = parser.ParseInteger() },
            { "RandomizeStartFrame", (parser, x) => x.RandomizeStartFrame = parser.ParseBoolean() },
            { "NumberImages", (parser, x) => parser.ParseInteger() },
            { "Image", (parser, x) => x.Images.Add(parser.ParseAssetReference()) },
        };

        public string Name { get; private set; }

        public AnimationMode AnimationMode { get; private set; }
        public int AnimationDelay { get; private set; }
        public bool RandomizeStartFrame { get; private set; }
        public List<string> Images { get; } = new List<string>();
    }

    public enum AnimationMode
    {
        [IniEnum("ONCE")]
        Once,

        [IniEnum("ONCE_BACKWARDS")]
        OnceBackwards,

        [IniEnum("LOOP")]
        Loop,

        [IniEnum("LOOP_BACKWARDS")]
        LoopBackwards,

        [IniEnum("PING_PONG")]
        PingPong,

        [IniEnum("MANUAL")]
        Manual
    }
}
