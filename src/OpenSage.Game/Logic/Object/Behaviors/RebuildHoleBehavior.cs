using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the REBUILD_HOLE KindOf on the object that will use this to work properly.
    /// </summary>
    public sealed class RebuildHoleBehaviorModuleData : BehaviorModuleData
    {
        internal static RebuildHoleBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<RebuildHoleBehaviorModuleData> BaseFieldParseTable = new IniParseTable<RebuildHoleBehaviorModuleData>
        {
            { "WorkerObjectName", (parser, x) => x.WorkerObjectName = parser.ParseAssetReference() },
            { "WorkerRespawnDelay", (parser, x) => x.WorkerRespawnDelay = parser.ParseInteger() },
            { "HoleHealthRegen%PerSecond", (parser, x) => x.HoleHealthRegenPercentPerSecond = parser.ParsePercentage() }
        };

        public string WorkerObjectName { get; private set; }

        public int WorkerRespawnDelay { get; private set; }

        public Percentage HoleHealthRegenPercentPerSecond { get; private set; }
    }
}
