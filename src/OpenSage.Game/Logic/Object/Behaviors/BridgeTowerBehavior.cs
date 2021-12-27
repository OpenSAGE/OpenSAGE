using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class BridgeTowerBehavior : BehaviorModule
    {
        private int _unknown1;
        private int _unknown2;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _unknown1 = reader.ReadInt32();
            _unknown2 = reader.ReadInt32();
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
