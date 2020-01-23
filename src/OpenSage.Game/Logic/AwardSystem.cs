using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AwardSystem : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, AwardSystem value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<AwardSystem> FieldParseTable = new IniParseTable<AwardSystem>
        {
            { "ObjectAward", (parser, x) => x.ObjectAwards.Add(ObjectAward.Parse(parser)) },
            { "ThingStat", (parser, x) => x.ThingStats.Add(ThingStat.Parse(parser)) }
        };

        public List<ObjectAward> ObjectAwards { get; } = new List<ObjectAward>();
        public List<ThingStat> ThingStats { get; } = new List<ThingStat>();
    }

    public sealed class ObjectAward
    {
        internal static ObjectAward Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<ObjectAward> FieldParseTable = new IniParseTable<ObjectAward>
        {
            { "AwardName", (parser, x) => x.AwardName = parser.ParseString() },
            { "ImageName", (parser, x) => x.ImageName = parser.ParseAssetReference() },
            { "NameTag", (parser, x) => x.NameTag = parser.ParseLocalizedStringKey() },
            { "DescriptionTag", (parser, x) => x.DescriptionTag = parser.ParseLocalizedStringKey() },
            { "Trigger", (parser, x) => x.Triggers.Add(Trigger.Parse(parser)) }
        };

        public string AwardName { get; private set; }
        public string ImageName { get; private set; }
        public string NameTag { get; private set; }
        public string DescriptionTag { get; private set; }
        public List<Trigger> Triggers { get; } = new List<Trigger>();
    }

    public sealed class Trigger
    {
        internal static Trigger Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<Trigger> FieldParseTable = new IniParseTable<Trigger>
        {
            { "Stat", (parser, x) => x.ThingStats = parser.ParseAssetReferenceArray() },
            { "Threshold", (parser, x) => x.Threshold = parser.ParseInteger() },
        };
        public string[] ThingStats { get; private set; }
        public int Threshold { get; private set; }
    }

    public sealed class ThingStat
    {
        internal static ThingStat Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<ThingStat> FieldParseTable = new IniParseTable<ThingStat>
        {
            { "StatName", (parser, x) => x.StatName = parser.ParseString() },
            { "NameTag", (parser, x) => x.NameTag = parser.ParseLocalizedStringKey() },
            { "DescriptionTag", (parser, x) => x.DescriptionTag = parser.ParseLocalizedStringKey() },
            { "ThingTemplateNames", (parser, x) => x.ThingTemplateNames.Add(parser.ParseAssetReferenceArray()) },
            { "KindOf", (parser, x) => x.ObjectKinds = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ExcludedKindOf", (parser, x) => x.ExcludedKindOf = parser.ParseEnumBitArray<ObjectKinds>() }
        };

        public string StatName { get; private set; }
        public string NameTag { get; private set; }
        public string DescriptionTag { get; private set; }
        public List<string[]> ThingTemplateNames { get; } = new List<string[]>();
        public BitArray<ObjectKinds> ObjectKinds { get; private set; }
        public BitArray<ObjectKinds> ExcludedKindOf { get; private set; }
    }
}
