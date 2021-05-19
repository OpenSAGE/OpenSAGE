using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Terrain
{
    public sealed class TerrainLogic
    {
        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            var unknown = reader.ReadInt32();
            if (unknown != 2u)
            {
                throw new InvalidDataException();
            }

            var unknown2 = reader.ReadInt32();
            if (unknown2 != 0u)
            {
                throw new InvalidDataException();
            }

            var unknown3 = reader.ReadByte();
            if (unknown3 != 0)
            {
                throw new InvalidDataException();
            }
        }
    }
}
