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

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadVector3(ref _unknownPosition1);
            reader.ReadUInt32(ref _unknownInt1);
            reader.ReadBoolean(ref _unknownBool1);
            reader.ReadVector3(ref _unknownPosition2);
            reader.ReadUInt32(ref _unknownInt2);
            reader.ReadUInt32(ref _unknownInt3);
            reader.ReadBoolean(ref _unknownBool2);
        }
    }
}
