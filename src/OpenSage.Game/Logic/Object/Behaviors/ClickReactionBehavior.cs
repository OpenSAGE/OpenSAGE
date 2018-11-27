using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class ClickReactionBehaviorData : BehaviorModuleData
    {
        internal static ClickReactionBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ClickReactionBehaviorData> FieldParseTable = new IniParseTable<ClickReactionBehaviorData>
        {
            { "ClickReactionTimer", (parser, x) => x.ClickReactionTimer = parser.ParseInteger() },
            { "ReactionFrames1", (parser, x) => x.ReactionFrames1 = parser.ParseInteger() },
            { "ReactionFrames2", (parser, x) => x.ReactionFrames2 = parser.ParseInteger() },
            { "ReactionFrames3", (parser, x) => x.ReactionFrames3 = parser.ParseInteger() },
            { "ReactionFrames4", (parser, x) => x.ReactionFrames4 = parser.ParseInteger() },
            { "ReactionFrames5", (parser, x) => x.ReactionFrames5 = parser.ParseInteger() },
        };

        public int ClickReactionTimer { get; private set; }
        public int ReactionFrames1 { get; private set; }
        public int ReactionFrames2 { get; private set; }
        public int ReactionFrames3 { get; private set; }
        public int ReactionFrames4 { get; private set; }
        public int ReactionFrames5 { get; private set; }
    }
}
