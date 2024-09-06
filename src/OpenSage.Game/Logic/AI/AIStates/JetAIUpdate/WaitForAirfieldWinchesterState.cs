#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

/// <summary>
/// Used when an aircraft is out of ammunition and needs to find a place to land, but there is no airfield available.
/// </summary>
internal sealed class WaitForAirfieldWinchesterState : State
{
    public const uint StateId = 1013;

    private readonly JetAIUpdate _aiUpdate;

    internal WaitForAirfieldWinchesterState(GameObject gameObject, GameContext context, JetAIUpdate aiUpdate) : base(gameObject, context)
    {
        _aiUpdate = aiUpdate;
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
