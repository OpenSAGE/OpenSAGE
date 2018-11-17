using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AnimationSoundClientBehaviorData : ClientBehaviorModuleData
    {
        internal static AnimationSoundClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<AnimationSoundClientBehaviorData> FieldParseTable = new IniParseTable<AnimationSoundClientBehaviorData>
        {
            { "MaxUpdateRangeCap", (parser, x) => x.MaxUpdateRangeCap = parser.ParseInteger() },
            { "AnimationSound", (parser, x) => x.AnimationSounds.Add(AnimationSoundData.Parse(parser)) },
        };

        public int MaxUpdateRangeCap { get; private set; }
		public List<AnimationSoundData> AnimationSounds { get; private set; } = new List<AnimationSoundData>();
    }
}
