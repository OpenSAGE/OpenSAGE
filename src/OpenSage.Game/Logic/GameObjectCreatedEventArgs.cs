using System;
using OpenSage.Logic.Object;

namespace OpenSage.Logic;

public sealed class GameObjectCreatedEventArgs : EventArgs
{
    public GameObject GameObject { get; }

    internal GameObjectCreatedEventArgs(GameObject gameObject)
    {
        GameObject = gameObject;
    }
}
