using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to hunt using a particular ability or command via scripts. AI only.
    /// </summary>
    public sealed class CommandButtonHuntUpdateModuleData : UpdateModuleData
    {
        internal static CommandButtonHuntUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CommandButtonHuntUpdateModuleData> FieldParseTable = new IniParseTable<CommandButtonHuntUpdateModuleData>();
    }
}
