﻿using System.Numerics;

namespace OpenSage.FX;

internal sealed class FXListExecutionContext
{
    public readonly Quaternion Rotation;
    public readonly Vector3 Position;
    public readonly IGameEngine GameEngine;

    public FXListExecutionContext(
        in Quaternion rotation,
        in Vector3 position,
        IGameEngine gameEngine)
    {
        Rotation = rotation;
        Position = position;
        GameEngine = gameEngine;
    }
}
