using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Terrain
{
    public sealed class TerrainLogic
    {
        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadVersion(2);

            var unknown = reader.ReadInt32();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }

            var unknown2 = reader.ReadInt32();
            if (unknown2 != 0)
            {
                throw new InvalidDataException();
            }
        }
    }
}
