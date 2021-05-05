using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class TechBuildingBehavior : UpdateModule
    {
        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
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
