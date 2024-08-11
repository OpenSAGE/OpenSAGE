using System.Numerics;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class ChinookTakeoffAndLandingState : State
{
    private Vector3 _unknownPosition;
    private bool _landing;

    public ChinookTakeoffAndLandingState(bool landing)
    {
        _landing = landing;
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistVector3(ref _unknownPosition);
        reader.PersistBoolean(ref _landing);
    }
}
