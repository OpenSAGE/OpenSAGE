using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class CommandButtonHuntUpdate : UpdateModule
    {
        // TODO

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown = reader.ReadByte();
        }
    }

    /// <summary>
    /// Allows this object to hunt using a particular ability or command via scripts. AI only.
    /// </summary>
    public sealed class CommandButtonHuntUpdateModuleData : UpdateModuleData
    {
        internal static CommandButtonHuntUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CommandButtonHuntUpdateModuleData> FieldParseTable = new IniParseTable<CommandButtonHuntUpdateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CommandButtonHuntUpdate();
        }
    }
}
