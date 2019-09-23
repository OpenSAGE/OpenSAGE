using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class RandomSoundSelectorClientBehaviorData : ClientBehaviorModuleData
    {
        internal static RandomSoundSelectorClientBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<RandomSoundSelectorClientBehaviorData> FieldParseTable = new IniParseTable<RandomSoundSelectorClientBehaviorData>
        {
            { "Chance", (parser, x) => x.Chance = parser.ParsePercentage() },
            { "RerollOnEveryFrame", (parser, x) => x.RerollOnEveryFrame = parser.ParseBoolean() },
            { "VoicePriority", (parser, x) => x.VoicePriority = parser.ParseInteger() }
        };

        public Percentage Chance { get; private set; }
        public bool RerollOnEveryFrame { get; private set; }
        public int VoicePriority { get; private set; }
    }
}
