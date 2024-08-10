using System.Numerics;

namespace OpenSage.Logic.Object;

public class RallyPointManager : IPersistableObject
{
    public Vector3? RallyPoint => _isRallyPointSet ? _rallyPoint : null;

    private Vector3 _rallyPoint;
    private bool _isRallyPointSet;

    public void Persist(StatePersister reader)
    {
        reader.PersistVector3(ref _rallyPoint);
        reader.PersistBoolean(ref _isRallyPointSet);
    }

    public void SetRallyPoint(Vector3? rallyPoint)
    {
        _rallyPoint = rallyPoint ?? default;
        _isRallyPointSet = rallyPoint.HasValue;
    }
}
