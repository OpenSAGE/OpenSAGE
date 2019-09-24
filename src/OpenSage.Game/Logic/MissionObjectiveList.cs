using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class MissionObjectiveList : BaseSingletonAsset
    {
        internal static void Parse(IniParser parser, MissionObjectiveList value) => parser.ParseBlockContent(value, FieldParseTable);

        private static readonly IniParseTable<MissionObjectiveList> FieldParseTable = new IniParseTable<MissionObjectiveList>
        {
            { "MissionObjectiveTag", (parser, x) => x.MissionObjectiveTags.Add(parser.ParseLocalizedStringKey()) },
            { "BonusMissionObjectiveTag", (parser, x) => x.BonusMissionObjectiveTags.Add(parser.ParseLocalizedStringKey()) }
        };

        public List<string> MissionObjectiveTags { get; } = new List<string>();
        public List<string> BonusMissionObjectiveTags { get; } = new List<string>();
    }
}
