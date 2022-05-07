using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class PowerPlantUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly PowerPlantUpdateModuleData _moduleData;

        private bool _rodsExtended;

        // TODO: This should be in frame numbers.
        private TimeSpan _rodsExtendedEndTime;

        internal PowerPlantUpdate(GameObject gameObject, PowerPlantUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal void ExtendRods()
        {
            _rodsExtended = true;

            _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgrading, true);

            _rodsExtendedEndTime = _gameObject.GameContext.Scene3D.Game.GetTimeInterval().TotalTime + _moduleData.RodsExtendTime;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            if (_rodsExtended && _rodsExtendedEndTime < context.Time.TotalTime)
            {
                _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgrading, false);
                _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgraded, true);
                _rodsExtendedEndTime = TimeSpan.MaxValue;
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistBoolean(ref _rodsExtended);
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
            return new PowerPlantUpdate(gameObject, this);
        }
    }
}
