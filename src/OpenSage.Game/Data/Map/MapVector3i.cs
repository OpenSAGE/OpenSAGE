using System.IO;

namespace OpenSage.Data.Map
{
    public struct MapVector3i
    {
        public int X;
        public int Y;
        public int Z;

        internal static MapVector3i Parse(BinaryReader reader)
        {
            return new MapVector3i
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}
