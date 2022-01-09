using System.Numerics;

namespace OpenSage.Logic.AI.AIStates
{
    internal class MoveTowardsState : State
    {
        private Vector3 _unknownPosition1;
        private uint _unknownInt1;
        private bool _unknownBool1;
        private Vector3 _unknownPosition2;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private bool _unknownBool2;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVector3(ref _unknownPosition1);
            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistVector3(ref _unknownPosition2);
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }
}
