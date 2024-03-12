using System.Numerics;

namespace OpenSage.Logic.Object;

public sealed class UnknownStateData : IPersistableObject
{
    private int _unknownInt1;
    private int _unknownInt2;

    public Vector3 TargetPosition;
    private uint _stateTargetObjectId;

    private uint _unknownUInt1;
    private uint _unknownUInt2;

    private bool _unknownBool1;
    private bool _unknownBool2;
    private bool _unknownBool3;

    public void Persist(StatePersister persister)
    {
        // these seem to have something to do with the current or next state
        // 11 for some jets?
        // 12 when attack structure
        // 14 when ...?
        // 15 when attack move order queued
        // 29 when guard order queued
        persister.PersistInt32(ref _unknownInt1);
        persister.PersistInt32(ref _unknownInt2);

        if (_unknownInt1 != _unknownInt2)
        {
            throw new InvalidStateException();
        }

        persister.PersistVector3(ref TargetPosition); // the purpose of this target seems to vary based on the next state
        persister.PersistObjectID(ref _stateTargetObjectId); // the purpose of this object seems to vary based on the next state

        persister.SkipUnknownBytes(9);

        persister.PersistUInt32(ref _unknownUInt1);
        if (_unknownUInt1 != 0x7FFFFFFF)
        {
            throw new InvalidStateException();
        }

        persister.SkipUnknownBytes(1);

        persister.PersistUInt32(ref _unknownUInt2);
        if (_unknownUInt2 != 0 && _unknownUInt2 != 0x7FFFFFFF)
        {
            throw new InvalidStateException();
        }

        persister.PersistBoolean(ref _unknownBool1);
        if (!_unknownBool1)
        {
            throw new InvalidStateException();
        }

        persister.PersistBoolean(ref _unknownBool2);
        if (!_unknownBool2)
        {
            throw new InvalidStateException();
        }

        persister.SkipUnknownBytes(18);

        persister.PersistBoolean(ref _unknownBool3);
        if (!_unknownBool3)
        {
            throw new InvalidStateException();
        }

        persister.SkipUnknownBytes(11);
    }
}
