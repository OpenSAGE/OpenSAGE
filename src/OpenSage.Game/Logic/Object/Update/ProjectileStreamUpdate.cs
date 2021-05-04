using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class ProjectileStreamUpdate : UpdateModule
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
