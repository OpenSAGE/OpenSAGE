namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FaceState : State
    {
        private readonly FaceTargetType _targetType;

        public FaceState(FaceTargetType targetType)
        {
            _targetType = targetType;
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool = true;
            reader.PersistBoolean("UnknownBool", ref unknownBool);
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
