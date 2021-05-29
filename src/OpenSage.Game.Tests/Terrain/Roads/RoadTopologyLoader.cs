using System.Collections.Generic;
using OpenSage.Data.Map;
using OpenSage.Terrain.Roads;

namespace OpenSage.Tests.Terrain.Roads
{
    internal static class RoadTopologyLoader
    {
        public static RoadTopology FromMapObjects(MapObject[] mapObjects)
        {
            var roadTopology = new RoadTopology();
            var templates = new Dictionary<string, RoadTemplate>();

            for (var i = 0; i < mapObjects.Length; i++)
            {
                var mapObject = mapObjects[i];

                switch (mapObject.RoadType & RoadType.PrimaryType)
                {
                    case RoadType.Start:
                    case RoadType.End:
                        var roadEnd = mapObjects[++i];

                        // Some maps have roads with invalid start- or endpoints.
                        // We'll skip processing them altogether.
                        if (mapObject.TypeName == "" || roadEnd.TypeName == "")
                        {
                            continue;
                        }

                        if (!templates.TryGetValue(mapObject.TypeName, out var template))
                        {
                            template = new RoadTemplate(mapObject.TypeName);
                            templates.Add(mapObject.TypeName, template);
                        }

                        roadTopology.AddSegment(template, mapObject, roadEnd);
                        break;

                }
            }

            return roadTopology;
        }
    }
}
