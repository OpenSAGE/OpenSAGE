using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class CastlePerimeter
    {
        public bool HasPerimeter { get; private set; }

        public CastlePerimeterPoint[] PerimeterPoints { get; set; }

        internal static CastlePerimeter Parse(BinaryReader reader, ushort version)
        {
            var result = new CastlePerimeter
            {
                HasPerimeter = reader.ReadBooleanUInt32Checked()
            };

            if (result.HasPerimeter)
            {
                var numPerimeterPoints = reader.ReadUInt32();
                result.PerimeterPoints = new CastlePerimeterPoint[numPerimeterPoints];

                for (var i = 0; i < numPerimeterPoints; i++)
                {
                    result.PerimeterPoints[i] = CastlePerimeterPoint.Parse(reader, version);
                }
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            writer.WriteBooleanUInt32(HasPerimeter);

            if (HasPerimeter)
            {
                writer.Write((uint) PerimeterPoints.Length);
                for (var i = 0; i < PerimeterPoints.Length; i++)
                {
                    PerimeterPoints[i].WriteTo(writer, version);
                }
            }
        }
    }
}
