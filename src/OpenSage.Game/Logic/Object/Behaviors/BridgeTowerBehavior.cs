using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class BridgeTowerBehavior : BehaviorModule
    {
        private int _unknown1;
        private int _unknown2;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistInt32(ref _unknown1);
            reader.PersistInt32(ref _unknown2);
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
