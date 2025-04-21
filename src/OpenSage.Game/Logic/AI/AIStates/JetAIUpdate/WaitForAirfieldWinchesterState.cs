#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

/// <summary>
/// Used when an aircraft is out of ammunition and needs to find a place to land, but there is no airfield available.
/// </summary>
internal sealed class WaitForAirfieldWinchesterState : State
{
    internal WaitForAirfieldWinchesterState(JetAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
