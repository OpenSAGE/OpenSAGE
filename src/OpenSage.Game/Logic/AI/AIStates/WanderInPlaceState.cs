using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class WanderInPlaceState : MoveTowardsState
    {
        private Vector3 _unknownPos;
        private uint _unknownInt;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistVector3("UnknownPos", ref _unknownPos);
            reader.PersistUInt32("UnknownInt", ref _unknownInt);

            reader.SkipUnknownBytes(4);
        }
    }
}
