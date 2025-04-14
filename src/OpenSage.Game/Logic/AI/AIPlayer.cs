using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

public class AIPlayer : IPersistableObject
{
    private readonly Player _owner;

    private readonly List<AIPlayerUnknownThing> _unknownThings = new();
    private readonly List<AIPlayerUnknownThing> _unknownThings2 = new();
    private bool _readyToBuildTeam;
    private bool _readyToBuildStructure;
    private int _teamTimer;
    private int _structureTimer;
    private int _buildDelay;
    private int _teamDelay;
    private int _teamSeconds;
    private ObjectId _curWarehouseId;
    private uint _frameLastBuildingBuilt;
    private Difficulty _difficulty;
    private int _skillsetSelector;
    private Vector3 _baseCenter;
    private bool _baseCenterSet;
    private float _baseRadius;

    // TODO(Port): m_structuresToRepair, m_repairDozer, m_structuresInQueue, m_dozerQueuedForRepair, m_dozerIsRepairing, m_bridgeTimer

    public Difficulty Difficulty
    {
        get => _difficulty;
        internal set => _difficulty = value;
    }

    internal AIPlayer(Player owner)
    {
        _owner = owner;
    }

    public virtual void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistList(
            _unknownThings,
            static (StatePersister persister, ref AIPlayerUnknownThing item) =>
            {
                item ??= new AIPlayerUnknownThing();
                persister.PersistObjectValue(item);
            });

        reader.PersistList(
            _unknownThings2,
            static (StatePersister persister, ref AIPlayerUnknownThing item) =>
            {
                item ??= new AIPlayerUnknownThing();
                persister.PersistObjectValue(item);
            });

        var playerId = _owner.Index;
        reader.PersistPlayerIndex(ref playerId);
        if (playerId != _owner.Index)
        {
            throw new InvalidStateException();
        }

        reader.PersistBoolean(ref _readyToBuildTeam);
        reader.PersistBoolean(ref _readyToBuildStructure);

        reader.PersistInt32(ref _teamTimer);
        if (_teamTimer != 2 && _teamTimer != 0)
        {
            throw new InvalidStateException();
        }

        reader.PersistInt32(ref _structureTimer);
        if (_structureTimer != 0 && _structureTimer != -1 && _structureTimer != 1)
        {
            throw new InvalidStateException();
        }

        reader.PersistInt32(ref _buildDelay); // Decrements by 1 each logic frame. When it reaches 0, it resets to 60.
        reader.PersistInt32(ref _teamDelay); // Decrements by 1 each logic frame. When it reaches 0, it resets to 150.

        reader.PersistInt32(ref _teamSeconds);
        if (_teamSeconds != 10)
        {
            throw new InvalidDataException();
        }

        reader.PersistObjectId(ref _curWarehouseId);
        reader.PersistUInt32(ref _frameLastBuildingBuilt); // 0, 1

        reader.PersistEnum(ref _difficulty);
        if (!Enum.IsDefined(_difficulty))
        {
            throw new InvalidStateException();
        }

        reader.PersistInt32(ref _skillsetSelector);
        if (_skillsetSelector != -1 && _skillsetSelector != 0 && _skillsetSelector != 1)
        {
            throw new InvalidStateException();
        }

        reader.PersistVector3(ref _baseCenter);
        reader.PersistBoolean(ref _baseCenterSet);
        reader.PersistSingle(ref _baseRadius);

        reader.SkipUnknownBytes(22);
    }

    private sealed class AIPlayerUnknownThing : IPersistableObject
    {
        private readonly List<AIPlayerUnknownOtherThing> _unknownThings = new();
        private bool _unknownBool;
        private uint _unknownInt1;
        private uint _unknownInt2;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistList(
                _unknownThings, static (StatePersister persister, ref AIPlayerUnknownOtherThing item) =>
                {
                    item ??= new AIPlayerUnknownOtherThing();
                    persister.PersistObjectValue(item);
                });

            reader.PersistBoolean(ref _unknownBool);
            reader.PersistUInt32(ref _unknownInt1); // 11
            reader.PersistUInt32(ref _unknownInt2);

            reader.SkipUnknownBytes(7);
        }
    }

    private sealed class AIPlayerUnknownOtherThing : IPersistableObject
    {
        private string _objectName;
        private ObjectId _objectId;
        private uint _unknownInt1;
        private uint _unknownInt2;
        private bool _unknownBool1;
        private bool _unknownBool2;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistAsciiString(ref _objectName);
            reader.PersistObjectId(ref _objectId);
            reader.PersistUInt32(ref _unknownInt1); // 0
            reader.PersistUInt32(ref _unknownInt2); // 1
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }
}
