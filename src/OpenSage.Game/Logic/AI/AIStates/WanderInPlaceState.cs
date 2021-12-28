using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class WanderInPlaceState : MoveTowardsState
    {
        private Vector3 _unknownPos;
        private uint _unknownInt;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadVector3(ref _unknownPos);

            _unknownInt = reader.ReadUInt32();

            reader.SkipUnknownBytes(4);
        }
    }
}
