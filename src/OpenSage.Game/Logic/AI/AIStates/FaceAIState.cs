using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FaceAIState : State
    {
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
}
