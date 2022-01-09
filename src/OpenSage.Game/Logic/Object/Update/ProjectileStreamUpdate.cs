using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ProjectileStreamUpdate : UpdateModule
    {
        private readonly uint[] _objectIds = new uint[20];
        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownObjectId;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistArray(
                _objectIds, static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistObjectID(ref _unknownObjectId);
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
