using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class PowerPlantUpdate : UpdateModule
    {
        private bool _rodsExtended;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistBoolean("RodsExtended", ref _rodsExtended);
        }
    }

    /// <summary>
    /// Allows this object to act as an (upgradeable) power supply, and allows this object to use 
    /// the <see cref="ModelConditionFlag.PowerPlantUpgrading"/> and 
    /// <see cref="ModelConditionFlag.PowerPlantUpgraded"/> model condition states.
    /// </summary>
    public sealed class PowerPlantUpdateModuleData : UpdateModuleData
    {
        internal static PowerPlantUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PowerPlantUpdateModuleData> FieldParseTable = new IniParseTable<PowerPlantUpdateModuleData>
        {
            { "RodsExtendTime", (parser, x) => x.RodsExtendTime = parser.ParseTimeMilliseconds() }
        };

        public TimeSpan RodsExtendTime { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PowerPlantUpdate();
        }
    }
}
