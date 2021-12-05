using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AnimationTemplate : BaseAsset
    {
        internal static AnimationTemplate Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Animation", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<AnimationTemplate> FieldParseTable = new IniParseTable<AnimationTemplate>
        {
            { "AnimationMode", (parser, x) => x.AnimationMode = parser.ParseEnum<AnimationMode>() },
            { "AnimationDelay", (parser, x) => x.AnimationDelay = parser.ParseInteger() },
            { "RandomizeStartFrame", (parser, x) => x.RandomizeStartFrame = parser.ParseBoolean() },
            { "NumberImages", (parser, x) => parser.ParseInteger() },
            { "Image", (parser, x) => x.Images.Add(parser.ParseAssetReference()) },
        };

        public AnimationMode AnimationMode { get; private set; }
        public int AnimationDelay { get; private set; }
        public bool RandomizeStartFrame { get; private set; }
        public List<string> Images { get; } = new List<string>();
    }
}
