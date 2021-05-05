using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class BridgeTowerBehavior : BehaviorModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            // TODO
        }
    }

    /// <summary>
    /// Transfers damage done to itself to its parent bridge too.
    /// </summary>
    public sealed class BridgeTowerBehaviorModuleData : BehaviorModuleData
    {
        internal static BridgeTowerBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BridgeTowerBehaviorModuleData> FieldParseTable = new IniParseTable<BridgeTowerBehaviorModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new BridgeTowerBehavior();
        }
    }
}
