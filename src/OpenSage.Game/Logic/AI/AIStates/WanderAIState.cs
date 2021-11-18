using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI
{
    internal sealed class WanderAIState : AIState1
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownPos = reader.ReadVector3();

            var unknownInt1 = reader.ReadUInt32();

            var unknownInt2 = reader.ReadUInt32();
            if (unknownInt2 != 0)
            {
                throw new InvalidDataException();
            }
        }
    }
}
