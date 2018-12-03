using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpawnBehaviorModuleData : UpgradeModuleData
    {
        internal static SpawnBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpawnBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<SpawnBehaviorModuleData>
            {
                { "SpawnNumber", (parser, x) => x.SpawnNumber = parser.ParseInteger() },
                { "SpawnReplaceDelay", (parser, x) => x.SpawnReplaceDelay = parser.ParseInteger() },
                { "SpawnTemplateName", (parser, x) => x.SpawnTemplateName = parser.ParseAssetReference() },
                { "OneShot", (parser, x) => x.OneShot = parser.ParseBoolean() },
                { "CanReclaimOrphans", (parser, x) => x.CanReclaimOrphans = parser.ParseBoolean() },
                { "SpawnedRequireSpawner", (parser, x) => x.SpawnedRequireSpawner = parser.ParseBoolean() },
                { "ExitByBudding", (parser, x) => x.ExitByBudding = parser.ParseBoolean() },
                { "InitialBurst", (parser, x) => x.InitialBurst = parser.ParseInteger() },
                { "AggregateHealth", (parser, x) => x.AggregateHealth = parser.ParseBoolean() },
                { "SlavesHaveFreeWill", (parser, x) => x.SlavesHaveFreeWill = parser.ParseBoolean() },
                { "RespectCommandLimit", (parser, x) => x.RespectCommandLimit = parser.ParseBoolean() }
            });

        public int SpawnNumber { get; private set; }
        public int SpawnReplaceDelay { get; private set; }
        public string SpawnTemplateName { get; private set; }
        public bool OneShot { get; private set; }
        public bool CanReclaimOrphans { get; private set; }
        public bool SpawnedRequireSpawner { get; private set; }
        public bool ExitByBudding { get; private set; }
        public int InitialBurst { get; private set; }
        public bool AggregateHealth { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool SlavesHaveFreeWill { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool RespectCommandLimit { get; private set; }
    }
}
