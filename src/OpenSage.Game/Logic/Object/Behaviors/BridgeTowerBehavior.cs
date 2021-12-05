using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class BridgeTowerBehavior : BehaviorModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown1 = reader.ReadInt32();
            var unknown2 = reader.ReadInt32();
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
