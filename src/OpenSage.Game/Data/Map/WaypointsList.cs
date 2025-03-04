﻿using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class WaypointsList : Asset
    {
        public const string AssetName = "WaypointsList";

        public WaypointPath[] WaypointPaths { get; private set; }

        internal static WaypointsList Parse(BinaryReader reader, MapParseContext context)
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

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint)WaypointPaths.Length);

                foreach (var waypointPath in WaypointPaths)
                {
                    waypointPath.WriteTo(writer);
                }
            });
        }
    }
}
