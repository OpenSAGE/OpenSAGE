using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenSage.Logic.AI
{
    public class AIPlayer : IPersistableObject
    {
        private readonly Player _owner;

        private readonly List<AIPlayerUnknownThing> _unknownThings = new();
        private readonly List<AIPlayerUnknownThing> _unknownThings2 = new();
        private bool _unknownBool1;
        private bool _unknownBool2;
        private uint _unknownInt1;
        private int _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownInt4;
        private uint _unknownObjectId;
        private uint _unknownInt5;
        private uint _unknownInt6;
        private int _unknownInt7;
        private Vector3 _unknownPosition;
        private bool _unknownBool3;
        private float _unknownFloat;

        internal AIPlayer(Player owner)
        {
            _owner = owner;
        }

        public virtual void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistList("UnknownThings", _unknownThings, static (StatePersister persister, ref AIPlayerUnknownThing item) =>
            {
                item ??= new AIPlayerUnknownThing();
                persister.PersistObjectValue(item);
            });

            reader.PersistList("UnknownThings2", _unknownThings2, static (StatePersister persister, ref AIPlayerUnknownThing item) =>
            {
                item ??= new AIPlayerUnknownThing();
                persister.PersistObjectValue(item);
            });

            var playerId = _owner.Id;
            reader.PersistUInt32("PlayerId", ref playerId);
            if (playerId != _owner.Id)
            {
                throw new InvalidStateException();
            }

            reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
            reader.PersistBoolean("UnknownBool2", ref _unknownBool2);

            reader.PersistUInt32("UnknownInt1", ref _unknownInt1);
            if (_unknownInt1 != 2 && _unknownInt1 != 0)
            {
                throw new InvalidStateException();
            }

            reader.PersistInt32("UnknownInt2", ref _unknownInt2);
            if (_unknownInt2 != 0 && _unknownInt2 != -1)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32("UnknownInt3", ref _unknownInt3); // 50, 51, 8, 35
            reader.PersistUInt32("UnknownInt4", ref _unknownInt4); // 0, 50

            var unknown6 = 10u;
            reader.PersistUInt32("Unknown6", ref unknown6);
            if (unknown6 != 10)
            {
                throw new InvalidDataException();
            }

            reader.PersistObjectID("UnknownObjectId", ref _unknownObjectId);
            reader.PersistUInt32("UnknownInt5", ref _unknownInt5); // 0, 1

            reader.PersistUInt32("UnknownInt6", ref _unknownInt6);
            if (_unknownInt6 != 1 && _unknownInt6 != 0 && _unknownInt6 != 2)
            {
                throw new InvalidStateException();
            }

            reader.PersistInt32("UnknownInt7", ref _unknownInt7);
            if (_unknownInt7 != -1 && _unknownInt7 != 0 && _unknownInt7 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistVector3("UnknownPosition", ref _unknownPosition);
            reader.PersistBoolean("UnknownBool3", ref _unknownBool3);
            reader.PersistSingle("UnknownFloat", ref _unknownFloat);

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

                reader.PersistList("UnknownThings", _unknownThings, static (StatePersister persister, ref AIPlayerUnknownOtherThing item) =>
                {
                    item ??= new AIPlayerUnknownOtherThing();
                    persister.PersistObjectValue(item);
                });

                reader.PersistBoolean("UnknownBool", ref _unknownBool);
                reader.PersistUInt32("UnknownInt1", ref _unknownInt1); // 11
                reader.PersistUInt32("UnknownInt2", ref _unknownInt2);

                reader.SkipUnknownBytes(7);
            }
        }

        private sealed class AIPlayerUnknownOtherThing : IPersistableObject
        {
            private string _objectName;
            private uint _objectId;
            private uint _unknownInt1;
            private uint _unknownInt2;
            private bool _unknownBool1;
            private bool _unknownBool2;

            public void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistAsciiString("ObjectName", ref _objectName);
                reader.PersistObjectID("ObjectId", ref _objectId);
                reader.PersistUInt32("UnknownInt1", ref _unknownInt1); // 0
                reader.PersistUInt32("UnknownInt2", ref _unknownInt2); // 1
                reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
                reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
            }
        }
    }
}
