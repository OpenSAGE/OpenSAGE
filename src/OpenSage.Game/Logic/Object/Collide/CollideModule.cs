﻿using System.Numerics;

namespace OpenSage.Logic.Object;

public abstract class CollideModule : BehaviorModule, ICollideModule
{
    protected CollideModule(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    // TODO: Make this abstract.
    public virtual void OnCollide(GameObject other, in Vector3 location, in Vector3 normal) { }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public abstract class CollideModuleData : BehaviorModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Collide;
}

public interface ICollideModule
{
    void OnCollide(GameObject other, in Vector3 location, in Vector3 normal);
}
