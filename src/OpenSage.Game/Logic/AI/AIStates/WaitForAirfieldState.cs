namespace OpenSage.Logic.AI.AIStates;

/// <summary>
/// Used when an aircraft is out of ammunition and needs to find a place to land, but there is no airfield available.
/// </summary>
internal sealed class WaitForAirfieldState : State
{
    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
