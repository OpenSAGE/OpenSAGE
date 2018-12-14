using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dVolumeRandomizer
    {
        public uint ClassID { get; private set; }
        public float Value1 { get; private set; }
        public float Value2 { get; private set; }
        public float Value3 { get; private set; }

        internal static W3dVolumeRandomizer Parse(BinaryReader reader)
        {
            var result = new W3dVolumeRandomizer
            {
                ClassID = reader.ReadUInt32(),
                Value1 = reader.ReadSingle(),
                Value2 = reader.ReadSingle(),
                Value3 = reader.ReadSingle(),
            };

            reader.ReadBytes(4 * sizeof(uint)); // Pad

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(ClassID);
            writer.Write(Value1);
            writer.Write(Value2);
            writer.Write(Value3);

            for (var i = 0; i < 4; i++)
            {
                writer.Write(0u);
            }
        }
    }
}
