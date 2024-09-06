using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class RappelState : State
{
    private float _unknownFloat1;
    private float _unknownFloat2;
    private bool _unknownBool;

    internal RappelState(GameObject gameObject, GameContext context) : base(gameObject, context)
    {
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistSingle(ref _unknownFloat1);
        reader.PersistSingle(ref _unknownFloat2);
        reader.PersistBoolean(ref _unknownBool);
    }
}
