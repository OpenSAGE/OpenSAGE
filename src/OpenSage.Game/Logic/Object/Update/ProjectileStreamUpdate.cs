using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ProjectileStreamUpdate : UpdateModule
    {
        private readonly uint[] _objectIds = new uint[20];
        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownObjectId;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            for (var i = 0; i < _objectIds.Length; i++)
            {
                reader.ReadObjectID(ref _objectIds[i]);
            }

            _unknownInt1 = reader.ReadUInt32();

            _unknownInt2 = reader.ReadUInt32();

            reader.ReadObjectID(ref _unknownObjectId);
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
