using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FaceAIState : State
    {
        private readonly FaceAIStateType _type;

        public FaceAIState(FaceAIStateType type)
        {
            _type = type;
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

    internal enum FaceAIStateType
    {
        FaceNamed,
        FaceWaypoint,
    }
}
