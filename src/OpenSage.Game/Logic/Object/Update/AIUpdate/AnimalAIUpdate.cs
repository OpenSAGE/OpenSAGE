using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AnimalAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new AnimalAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AnimalAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<AnimalAIUpdateModuleData>
            {
                { "FleeRange", (parser, x) => x.FleeRange = parser.ParseInteger() },
                { "FleeDistance", (parser, x) => x.FleeDistance = parser.ParseInteger() },
                { "WanderPercentage", (parser, x) => x.WanderPercentage = parser.ParsePercentage() },
                { "MaxWanderDistance", (parser, x) => x.MaxWanderDistance = parser.ParseInteger() },
                { "MaxWanderRadius", (parser, x) => x.MaxWanderRadius = parser.ParseInteger() },
                { "UpdateTimer", (parser, x) => x.UpdateTimer = parser.ParseInteger() },
                { "AfraidOfCastles", (parser, x) => x.AfraidOfCastles = parser.ParseBoolean() }
            });

        public int FleeRange { get; private set; }
        public int FleeDistance { get; private set; }
        public Percentage WanderPercentage { get; private set; }
        public int MaxWanderDistance { get; private set; }
        public int MaxWanderRadius { get; private set; }
        public int UpdateTimer { get; private set; }
        public bool AfraidOfCastles { get; private set; }
    }
}
