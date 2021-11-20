using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class IdleState : State
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknownShort1 = reader.ReadUInt16();

            var unknownShort2 = reader.ReadUInt16();
            if (unknownShort2 != 1 && unknownShort2 != 0)
            {
                throw new InvalidDataException();
            }
        }
    }
}
