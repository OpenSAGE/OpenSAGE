using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI
{
    internal sealed class IdleAIState : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownShort1 = reader.ReadUInt16();

            var unknownShort2 = reader.ReadUInt16();
            if (unknownShort2 != 1)
            {
                throw new InvalidDataException();
            }
        }
    }
}
