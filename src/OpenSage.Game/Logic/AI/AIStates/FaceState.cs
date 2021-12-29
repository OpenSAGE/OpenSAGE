namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FaceState : State
    {
        private readonly FaceTargetType _targetType;

        public FaceState(FaceTargetType targetType)
        {
            _targetType = targetType;
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownBool = true;
            reader.ReadBoolean(ref unknownBool);
            if (!unknownBool)
            {
                throw new InvalidStateException();
            }
        }
    }

    internal enum FaceTargetType
    {
        FaceNamed,
        FaceWaypoint,
    }
}
