using System.IO;

namespace OpenZH.Data.Map
{
    public struct MapTexCoord
    {
        public float U;
        public float V;

        public static MapTexCoord Parse(BinaryReader reader)
        {
            return new MapTexCoord
            {
                U = reader.ReadSingle(),
                V = reader.ReadSingle()
            };
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(U);
            writer.Write(V);
        }
    }
}
