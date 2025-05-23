﻿using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics;

namespace OpenSage.Logic.Object;

public sealed class SpawnPointProductionExitUpdate : UpdateModule, IProductionExit
{
    private readonly SpawnPointProductionExitUpdateModuleData _moduleData;
    private int _nextIndex;

    internal SpawnPointProductionExitUpdate(GameObject gameObject, IGameEngine gameEngine, SpawnPointProductionExitUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        _nextIndex = 1;
    }

    Vector3 IProductionExit.GetUnitCreatePoint()
    {
        var (modelInstance, bone) = FindBone(_nextIndex);

        if (bone == null)
        {
            _nextIndex = 1;
            (modelInstance, bone) = FindBone(_nextIndex);

            if (bone == null)
            {
                throw new InvalidOperationException("Could not find spawn point bone");
            }
        }

        _nextIndex++;

        return modelInstance.RelativeBoneTransforms[bone.Index].Translation;
    }

    private (ModelInstance, ModelBone) FindBone(int index)
        => GameObject.Drawable.FindBone(_moduleData.SpawnPointBoneName + index.ToString("D2"));

    Vector3? IProductionExit.GetNaturalRallyPoint() => null;

    public override UpdateSleepTime Update()
    {
        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }
}

public sealed class SpawnPointProductionExitUpdateModuleData : UpdateModuleData
{
    internal static SpawnPointProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<SpawnPointProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<SpawnPointProductionExitUpdateModuleData>
    {
        { "SpawnPointBoneName", (parser, x) => x.SpawnPointBoneName = parser.ParseBoneName() }
    };

    public string SpawnPointBoneName { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new SpawnPointProductionExitUpdate(gameObject, gameEngine, this);
    }
}
