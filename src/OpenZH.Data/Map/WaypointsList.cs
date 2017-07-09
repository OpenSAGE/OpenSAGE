using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class WaypointsList : Asset
    {
        public WaypointPath[] WaypointPaths { get; private set; }

        public static WaypointsList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numWaypointPaths = reader.ReadUInt32();
                var waypointPaths = new WaypointPath[numWaypointPaths];

                for (var i = 0; i < numWaypointPaths; i++)
                {
                    waypointPaths[i] = WaypointPath.Parse(reader);
                }

                return new WaypointsList
                {
                    WaypointPaths = waypointPaths
                };
            });
        }
    }
}
