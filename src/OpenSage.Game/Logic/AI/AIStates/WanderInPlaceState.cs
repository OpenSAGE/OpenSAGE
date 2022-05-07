﻿using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class WanderInPlaceState : MoveTowardsState
    {
        private Vector3 _unknownPos;
        private uint _unknownInt;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistVector3(ref _unknownPos);
            reader.PersistUInt32(ref _unknownInt);

            reader.SkipUnknownBytes(4);
        }
    }
}
