using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ProjectileStreamUpdate : UpdateModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            // TODO
        }
    }

    /// <summary>
    /// Allows the object to behave as a stream like water or other liquid ordinance.
    /// </summary>
    public sealed class ProjectileStreamUpdateModuleData : UpdateModuleData
    {
        internal static ProjectileStreamUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProjectileStreamUpdateModuleData> FieldParseTable = new IniParseTable<ProjectileStreamUpdateModuleData>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ProjectileStreamUpdate();
        }
    }
}
