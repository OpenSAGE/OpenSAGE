﻿using System.Collections.Generic;
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
        private uint _countdownSomething1;
        private uint _countdownSomething2;
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

            var playerId = _owner.Id;
            reader.PersistUInt32( ref playerId);
            if (playerId != _owner.Id)
            {
                throw new InvalidStateException();
            }

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);

            reader.PersistUInt32(ref _unknownInt1);
            if (_unknownInt1 != 2 && _unknownInt1 != 0)
            {
                throw new InvalidStateException();
            }

            reader.PersistInt32(ref _unknownInt2);
            if (_unknownInt2 != 0 && _unknownInt2 != -1 && _unknownInt2 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _countdownSomething1); // Decrements by 1 each logic frame. When it reaches 0, it resets to 60.
            reader.PersistUInt32(ref _countdownSomething2); // Decrements by 1 each logic frame. When it reaches 0, it resets to 150.

            var unknown6 = 10u;
            reader.PersistUInt32(ref unknown6);
            if (unknown6 != 10)
            {
                throw new InvalidDataException();
            }

            reader.PersistObjectID(ref _unknownObjectId);
            reader.PersistUInt32(ref _unknownInt5); // 0, 1

            reader.PersistUInt32(ref _unknownInt6);
            if (_unknownInt6 != 1 && _unknownInt6 != 0 && _unknownInt6 != 2)
            {
                throw new InvalidStateException();
            }

            reader.PersistInt32(ref _unknownInt7);
            if (_unknownInt7 != -1 && _unknownInt7 != 0 && _unknownInt7 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistVector3(ref _unknownPosition);
            reader.PersistBoolean(ref _unknownBool3);
            reader.PersistSingle(ref _unknownFloat);

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
            private uint _objectId;
            private uint _unknownInt1;
            private uint _unknownInt2;
            private bool _unknownBool1;
            private bool _unknownBool2;

            public void Persist(StatePersister reader)
            {
                reader.PersistVersion(1);

                reader.PersistAsciiString(ref _objectName);
                reader.PersistObjectID(ref _objectId);
                reader.PersistUInt32(ref _unknownInt1); // 0
                reader.PersistUInt32(ref _unknownInt2); // 1
                reader.PersistBoolean(ref _unknownBool1);
                reader.PersistBoolean(ref _unknownBool2);
            }
        }
    }
}
