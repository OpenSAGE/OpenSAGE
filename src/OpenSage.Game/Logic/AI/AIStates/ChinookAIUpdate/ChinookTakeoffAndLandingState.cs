using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ChinookTakeoffAndLandingState : State
{
    private Vector3 _targetPosition;
    private bool _landing;

    public ChinookTakeoffAndLandingState(GameObject gameObject, GameContext context, ChinookAIUpdate aiUpdate, bool landing) : base(gameObject, context)
    {
        _landing = landing;
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistVector3(ref _targetPosition);
        reader.PersistBoolean(ref _landing);
    }
}
