using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FaceState : State
    {
        private readonly FaceTargetType _targetType;

        public FaceState(GameObject gameObject, GameContext context, FaceTargetType targetType) : base(gameObject, context)
        {
            _targetType = targetType;
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var unknownBool = true;
            reader.PersistBoolean(ref unknownBool);
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
