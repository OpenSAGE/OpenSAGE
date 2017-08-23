using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceNoBuild and VoiceTaskComplete 
    /// within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER.
    /// </summary>
    public sealed class DozerAIUpdate : ObjectBehavior
    {
        internal static DozerAIUpdate Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<DozerAIUpdate> BaseFieldParseTable = new IniParseTable<DozerAIUpdate>
        {
            { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
            { "BoredTime", (parser, x) => x.BoredTime = parser.ParseInteger() },
            { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = parser.ParseBoolean() }
        };

        public float RepairHealthPercentPerSecond { get; private set; }
        public int BoredTime { get; private set; }
        public int BoredRange { get; private set; }
        public bool AutoAcquireEnemiesWhenIdle { get; private set; }
    }
}
