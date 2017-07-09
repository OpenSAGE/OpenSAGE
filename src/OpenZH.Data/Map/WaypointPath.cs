using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class WaypointPath
    {
        public uint StartWaypointID { get; private set; }
        public uint EndWaypointID { get; private set; }

        public static WaypointPath Parse(BinaryReader reader)
        {
            return new WaypointPath
            {
                StartWaypointID = reader.ReadUInt32(),
                EndWaypointID = reader.ReadUInt32()
            };
        }
    }
}
