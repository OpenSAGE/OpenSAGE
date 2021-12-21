using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class TriggerArea
    {
        public string Name { get; private set; }
        public string LayerName { get; private set; }
        public uint ID { get; private set; }
        public Vector2[] Points { get; private set; }

        internal static TriggerArea Parse(BinaryReader reader)
        {
            var name = reader.ReadUInt16PrefixedAsciiString();

            var layerName = reader.ReadUInt16PrefixedAsciiString();

            var id = reader.ReadUInt32();

            var numPoints = reader.ReadUInt32();
            var points = new Vector2[numPoints];

            for (var i = 0; i < numPoints; i++)
            {
                points[i] = reader.ReadVector2();
            }

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0)
            {
                throw new InvalidDataException();
            }

            return new TriggerArea
            {
                Name = name,
                LayerName = layerName,
                ID = id,
                Points = points
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);

            writer.WriteUInt16PrefixedAsciiString(LayerName);

            writer.Write(ID);

            writer.Write((uint) Points.Length);
            foreach (var point in Points)
            {
                writer.Write(point);
            }

            writer.Write((uint) 0);
        }
    }
}
