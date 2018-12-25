using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LargeGroupAudioMap
    {
        internal static LargeGroupAudioMap Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LargeGroupAudioMap> FieldParseTable = new IniParseTable<LargeGroupAudioMap>
        {
            { "Sound", (parser, x) => x.Sounds.Add(LargeGroupAudioMapSound.Parse(parser)) },
            { "ModelConditionFlags", (parser, x) => x.RequiredModelConditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "ExcludedModelConditionFlags", (parser, x) => x.ExcludedModelConditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "ExcludedObjectStatusBits", (parser, x) => x.ExcludedObjectStatusBits = parser.ParseEnumBitArray<ObjectStatus>() },
            { "IgnoreStealthedUnits", (parser, x) => x.IgnoreStealthedUnits = parser.ParseBoolean() },
            { "Size", (parser, x) => x.Size = parser.ParseInteger() },
            { "StartThreshold", (parser, x) => x.StartThreshold = parser.ParseInteger() },
            { "StopThreshold", (parser, x) => x.StopThreshold = parser.ParseInteger() },
            { "HandOffModeDuration", (parser, x) => x.HandOffModeDuration = parser.ParseInteger() },
            { "MaximumAudioSpeed", (parser, x) => x.MaximumAudioSpeed = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public List<LargeGroupAudioMapSound> Sounds { get; } = new List<LargeGroupAudioMapSound>();
        public BitArray<ModelConditionFlag> RequiredModelConditionFlags { get; private set; }
        public BitArray<ModelConditionFlag> ExcludedModelConditionFlags { get; private set; }
        public BitArray<ObjectStatus> ExcludedObjectStatusBits { get; private set; }
        public bool IgnoreStealthedUnits { get; private set; } = true;
        public int Size { get; private set; }
        public int StartThreshold { get; private set; }
        public int StopThreshold { get; private set; }
        public int HandOffModeDuration { get; private set; }
        public int MaximumAudioSpeed { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LargeGroupAudioMapSound
    {
        internal static LargeGroupAudioMapSound Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LargeGroupAudioMapSound> FieldParseTable = new IniParseTable<LargeGroupAudioMapSound>
        {
            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() },
            { "Key", (parser, x) => x.Keys.AddRange(parser.ParseAssetReferenceArray()) },
        };

        public string Sound { get; private set; }
        public List<string> Keys { get; } = new List<string>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LargeGroupAudioUnusedKnownKeys
    {
        internal static LargeGroupAudioUnusedKnownKeys Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<LargeGroupAudioUnusedKnownKeys> FieldParseTable = new IniParseTable<LargeGroupAudioUnusedKnownKeys>
        {
            { "Key", (parser, x) => x.Keys.AddRange(parser.ParseAssetReferenceArray()) },
        };

        public List<string> Keys { get; } = new List<string>();
    }
}
