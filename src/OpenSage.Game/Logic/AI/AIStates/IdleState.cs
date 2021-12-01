namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class IdleState : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownShort1 = reader.ReadUInt16();

            var unknownBool1 = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();
        }
    }
}
