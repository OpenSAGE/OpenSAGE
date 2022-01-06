using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class TechBuildingBehavior : UpdateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    /// <summary>
    /// This module is required when KindOf contains TECH_BUILDING.
    /// </summary>
    public sealed class TechBuildingBehaviorModuleData : BehaviorModuleData
    {
        internal static TechBuildingBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TechBuildingBehaviorModuleData> FieldParseTable = new IniParseTable<TechBuildingBehaviorModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new TechBuildingBehavior();
        }
    }
}
