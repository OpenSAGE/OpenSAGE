using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal class MoveTowardsState : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var positionSomething = reader.ReadVector3();
            var unknownInt1 = reader.ReadUInt32();
            var unknownBool1 = reader.ReadBoolean();
            var positionSomething2 = reader.ReadVector3();
            var unknownInt2 = reader.ReadUInt32();
            var unknownInt3 = reader.ReadUInt32();
            var unknownBool2 = reader.ReadBoolean();
        }
    }
}
