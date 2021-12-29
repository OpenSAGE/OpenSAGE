namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class IdleState : State
    {
        private ushort _unknownShort;
        private bool _unknownBool1;
        private bool _unknownBool2;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadUInt16(ref _unknownShort);

            _unknownBool1 = reader.ReadBoolean();
            _unknownBool2 = reader.ReadBoolean();
        }
    }
}
