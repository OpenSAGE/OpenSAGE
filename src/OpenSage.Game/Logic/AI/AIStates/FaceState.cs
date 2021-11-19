using System.IO;
using OpenSage.Data.Sav;

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

            var unknownBool = reader.ReadBoolean();
            if (!unknownBool)
            {
                throw new InvalidDataException();
            }
        }
    }

    internal enum FaceTargetType
    {
        FaceNamed,
        FaceWaypoint,
    }
}
