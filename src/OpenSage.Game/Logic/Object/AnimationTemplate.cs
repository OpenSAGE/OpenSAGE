using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui;

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
            { "Image", (parser, x) => x.Images.Add(parser.ParseMappedImageReference()) },
        };

        public AnimationMode AnimationMode { get; set; }
        public int AnimationDelay { get; set; }
        public bool RandomizeStartFrame { get; set; }
        public List<LazyAssetReference<MappedImage>> Images { get; init; } = [];
    }
}
