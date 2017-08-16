using System.IO;

namespace OpenSage.Data.Map
{
    public struct MapTexCoord
    {
        public float U;
        public float V;

        internal static MapTexCoord Parse(BinaryReader reader)
        {
            return new MapTexCoord
            {
                U = reader.ReadSingle(),
                V = reader.ReadSingle()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(U);
            writer.Write(V);
        }
    }
}
