using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class PolygonTrigger
    {
        public string Name { get; private set; }
        public string LayerName { get; private set; }
        public uint UniqueId { get; private set; }
        public TriggerType TriggerType { get; private set; }
        public MapVector3i[] Points { get; private set; }

        public static PolygonTrigger Parse(BinaryReader reader)
        {
            var name = reader.ReadUInt16PrefixedAsciiString();
            var layerName = reader.ReadUInt16PrefixedAsciiString();

            var uniqueId = reader.ReadUInt32();

            var triggerType = reader.ReadUInt32AsEnum<TriggerType>();

            var unknown = reader.ReadUInt16();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }

            var numPoints = reader.ReadUInt32();
            var points = new MapVector3i[numPoints];

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
                Points = points
            };
        }
    }

    public enum TriggerType : uint
    {
        Area = 0,
        Water = 1
    }
}
