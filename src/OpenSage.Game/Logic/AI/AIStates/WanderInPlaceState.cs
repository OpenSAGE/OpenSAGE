using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class WanderInPlaceState : MoveTowardsState
    {
        private Vector3 _unknownPos;
        private uint _unknownInt;

        internal WanderInPlaceState(GameObject gameObject, GameContext context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
        {
        }

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
