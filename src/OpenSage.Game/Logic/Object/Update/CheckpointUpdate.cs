using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows object to open and close like a gate when a friendly object approaches it. Requires 
    /// <see cref="ModelConditionFlag.Door1Opening"/> and 
    /// <see cref="ModelConditionFlag.Door1Closing"/> condition states.
    /// </summary>
    public sealed class CheckpointUpdate : ObjectBehavior
    {
        internal static CheckpointUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CheckpointUpdate> FieldParseTable = new IniParseTable<CheckpointUpdate>();
    }
}
