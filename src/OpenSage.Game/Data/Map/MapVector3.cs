using System.IO;

namespace OpenSage.Data.Map
{
    public struct MapVector3
    {
        public float X;
        public float Y;
        public float Z;

        internal static MapVector3 Parse(BinaryReader reader)
        {
            return new MapVector3
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
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
