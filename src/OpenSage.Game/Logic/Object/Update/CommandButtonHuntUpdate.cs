using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to hunt using a particular ability or command via scripts. AI only.
    /// </summary>
    public sealed class CommandButtonHuntUpdate : ObjectBehavior
    {
        internal static CommandButtonHuntUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CommandButtonHuntUpdate> FieldParseTable = new IniParseTable<CommandButtonHuntUpdate>();
    }
}
