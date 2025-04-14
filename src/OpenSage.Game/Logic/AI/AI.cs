namespace OpenSage.Logic.AI;

public sealed class AI
{
    public readonly Pathfinder Pathfinder = new();
}

/// <summary>
/// Attitude types for teams.
/// </summary>
public enum AttitudeType
{
    Sleep = -2,
    Passive = -1,
    Normal = 0,
    Alert = 1,
    Aggressive = 2,
    Invalid = 3
}
