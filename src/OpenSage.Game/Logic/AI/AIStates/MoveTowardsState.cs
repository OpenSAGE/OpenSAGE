using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal class MoveTowardsState : State
    {
        private Vector3 _targetPosition;
        private uint _unknownInt1;
        private bool _unknownBool1;
        private Vector3 _unknownPosition2;
        private LogicFrame _unknownFrame;
        private uint _unknownInt3;
        private bool _unknownBool2;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVector3(ref _targetPosition);
            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistBoolean(ref _unknownBool1); // True until frame after path is ready in AIUpdate, then it's false
            reader.PersistVector3(ref _unknownPosition2); // Empty until frame after path is ready in AIUpdate, then it's the original path destination (original because the one in _targetPosition will have been modified to be at the centre of a terrain cell)
            reader.PersistLogicFrame(ref _unknownFrame); // 0 until frame X after path is ready in AIUpdate, then it's frame X
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }
}
