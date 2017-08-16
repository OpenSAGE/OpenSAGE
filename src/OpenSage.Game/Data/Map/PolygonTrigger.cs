using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class PolygonTrigger
    {
        public string Name { get; private set; }
        public string LayerName { get; private set; }
        public uint UniqueId { get; private set; }
        public PolygonTriggerType TriggerType { get; private set; }

        /// <summary>
        /// For rivers, this is the index into the array of Points
        /// for the point where the river flows from.
        /// </summary>
        public uint RiverStartControlPoint { get; private set; }

        public MapVector3i[] Points { get; private set; }

        internal static PolygonTrigger Parse(BinaryReader reader, ushort version)
        {
            var name = reader.ReadUInt16PrefixedAsciiString();

            string layerName = null;
            if (version == 4)
            {
                layerName = reader.ReadUInt16PrefixedAsciiString();
            }

            var uniqueId = reader.ReadUInt32();

            var triggerType = reader.ReadUInt16AsEnum<PolygonTriggerType>();

            var riverStartControlPoint = reader.ReadUInt32();

            var numPoints = reader.ReadUInt32();
            var points = new MapVector3i[numPoints];

            if (riverStartControlPoint > numPoints - 1)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < numPoints; i++)
            {
                points[i] = MapVector3i.Parse(reader);
            }

            return new PolygonTrigger
            {
                Name = name,
                LayerName = layerName,
                UniqueId = uniqueId,
                TriggerType = triggerType,
                RiverStartControlPoint = riverStartControlPoint,
                Points = points
            };
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);

            if (version == 4)
            {
                writer.WriteUInt16PrefixedAsciiString(LayerName);
            }

            writer.Write(UniqueId);

            writer.Write((ushort) TriggerType);

            writer.Write(RiverStartControlPoint);

            writer.Write((uint) Points.Length);

            foreach (var point in Points)
            {
                point.WriteTo(writer);
            }
        }
    }

    [Flags]
    public enum PolygonTriggerType : ushort
    {
        Area = 0,
        Water = 1,
        River = 256,

        WaterAndRiver = Water | River,
    }
}
