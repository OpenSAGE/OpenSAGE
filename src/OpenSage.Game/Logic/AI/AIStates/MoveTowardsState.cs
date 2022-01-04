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

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVector3("UnknownPosition1", ref _unknownPosition1);
            reader.PersistUInt32("UnknownInt1", ref _unknownInt1);
            reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
            reader.PersistVector3("UnknownPosition2", ref _unknownPosition2);
            reader.PersistUInt32("UnknownInt2", ref _unknownInt2);
            reader.PersistUInt32("UnknownInt3", ref _unknownInt3);
            reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
        }
    }
}
