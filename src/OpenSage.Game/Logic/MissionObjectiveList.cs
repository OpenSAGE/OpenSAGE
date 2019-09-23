using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class MissionObjectiveList
    {
        internal static MissionObjectiveList Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<MissionObjectiveList> FieldParseTable = new IniParseTable<MissionObjectiveList>
        {
            { "MissionObjectiveTag", (parser, x) => x.MissionObjectiveTags.Add(parser.ParseLocalizedStringKey()) },
            { "BonusMissionObjectiveTag", (parser, x) => x.BonusMissionObjectiveTags.Add(parser.ParseLocalizedStringKey()) }
        };

        public List<string> MissionObjectiveTags { get; } = new List<string>();
        public List<string> BonusMissionObjectiveTags { get; } = new List<string>();
    }
}
