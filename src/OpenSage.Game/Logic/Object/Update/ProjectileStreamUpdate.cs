﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class ProjectileStreamUpdate : UpdateModule
{
    private readonly ObjectId[] _objectIds = new ObjectId[20];
    private uint _unknownInt1;
    private uint _unknownInt2;
    private ObjectId _unknownObjectId;

    public ProjectileStreamUpdate(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    public override UpdateSleepTime Update()
    {
        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistArray(
            _objectIds,
            static (StatePersister persister, ref ObjectId item) =>
            {
                persister.PersistObjectIdValue(ref item);
            });

        reader.PersistUInt32(ref _unknownInt1);
        reader.PersistUInt32(ref _unknownInt2);
        reader.PersistObjectId(ref _unknownObjectId);
    }
}

/// <summary>
/// Allows the object to behave as a stream like water or other liquid ordinance.
/// </summary>
public sealed class ProjectileStreamUpdateModuleData : UpdateModuleData
{
    internal static ProjectileStreamUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<ProjectileStreamUpdateModuleData> FieldParseTable = new IniParseTable<ProjectileStreamUpdateModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new ProjectileStreamUpdate(gameObject, gameEngine);
    }
}
