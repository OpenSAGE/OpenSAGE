using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the REBUILD_HOLE KindOf on the object that will use this to work properly.
    /// </summary>
    public sealed class RebuildHoleBehavior : ObjectBehavior
    {
        internal static RebuildHoleBehavior Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<RebuildHoleBehavior> BaseFieldParseTable = new IniParseTable<RebuildHoleBehavior>
        {
            { "WorkerObjectName", (parser, x) => x.WorkerObjectName = parser.ParseAssetReference() },
            { "WorkerRespawnDelay", (parser, x) => x.WorkerRespawnDelay = parser.ParseInteger() },
            { "HoleHealthRegen%PerSecond", (parser, x) => x.HoleHealthRegenPercentPerSecond = parser.ParsePercentage() }
        };

        public string WorkerObjectName { get; private set; }

        public int WorkerRespawnDelay { get; private set; }

        public float HoleHealthRegenPercentPerSecond { get; private set; }
    }
}
