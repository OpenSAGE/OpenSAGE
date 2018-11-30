using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AnimationSoundData
    {
        internal static AnimationSoundData Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<AnimationSoundData> FieldParseTable = new IniParseTable<AnimationSoundData>
        {
            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() },
            { "Animation", (parser, x) => x.Animations.Add(parser.ParseAssetReference()) },
            { "Frames", (parser, x) => x.Frames.Add(parser.ParseIntegerArray()) },
            { "ExcludedMC", (parser, x) => x.ExcludedMC = parser.ParseEnum<ModelConditionFlag>() },
            { "RequiredMC", (parser, x) => x.RequiredMC = parser.ParseEnum<ModelConditionFlag>() }
        };

        public string Sound { get; private set; }
        public List<string> Animations { get; } = new List<string>();
        public List<int[]> Frames { get; } = new List<int[]>();
        public ModelConditionFlag ExcludedMC { get; private set; }
        public ModelConditionFlag RequiredMC { get; private set; }
        
    }
}
