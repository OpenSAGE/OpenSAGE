namespace OpenSage.Logic.AI.AIStates;

internal sealed class DoNothingState : State
{
    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);
    }
}
