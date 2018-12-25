using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class PillageModuleData : BehaviorModuleData
    {
        internal static PillageModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PillageModuleData> FieldParseTable = new IniParseTable<PillageModuleData>
            {
                { "PillageAmount", (parser, x) => x.PillageAmount = parser.ParseInteger() },
                { "NumDamageEventsPerPillage", (parser, x) => x.NumDamageEventsPerPillage = parser.ParseInteger() },
                { "PillageFilter", (parser, x) => x.PillageFilter = ObjectFilter.Parse(parser) },
            };

        public int PillageAmount { get; private set; }
        public int NumDamageEventsPerPillage { get; private set; }
        public ObjectFilter PillageFilter { get; private set; }
    }
}
