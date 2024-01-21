﻿using System;
using ImGuiNET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class PowerPlantUpdate : UpdateModule
    {
        private readonly GameObject _gameObject;
        private readonly PowerPlantUpdateModuleData _moduleData;

        private bool _rodsExtended;

        private LogicFrame _rodsExtendedEndFrame;

        internal PowerPlantUpdate(GameObject gameObject, PowerPlantUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal void ExtendRods()
        {
            _rodsExtended = true;

            _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgrading, true);

            _rodsExtendedEndFrame = _gameObject.GameContext.GameLogic.CurrentFrame + _moduleData.RodsExtendTime;
        }

        // China powerplant overcharge needs to be able to turn off
        internal void RetractRods()
        {
            _rodsExtended = false;
            _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgrading, false);
            _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgraded, false);

            _rodsExtendedEndFrame = LogicFrame.MaxValue;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            if (_rodsExtended && _rodsExtendedEndFrame < context.LogicFrame)
            {
                _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgrading, false);
                _gameObject.Drawable.ModelConditionFlags.Set(ModelConditionFlag.PowerPlantUpgraded, true);
                _rodsExtendedEndFrame = LogicFrame.MaxValue;
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

        internal override void DrawInspector()
        {
            ImGui.LabelText("Rods extended", _rodsExtended.ToString());
            if (ImGui.Button("Extend Rods"))
            {
                ExtendRods();
            }

            ImGui.SameLine();

            if (ImGui.Button("Retract Rods"))
            {
                RetractRods();
            }
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
            { "RodsExtendTime", (parser, x) => x.RodsExtendTime = parser.ParseTimeMillisecondsToLogicFrames() }
        };

        public LogicFrameSpan RodsExtendTime { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PowerPlantUpdate(gameObject, this);
        }
    }
}
