using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Map;
using OpenSage.Terrain.Roads;
using Xunit;
using static OpenSage.Data.Map.RoadType;
using static OpenSage.Tests.Terrain.Roads.CurveTypeTests.NodePosition;

namespace OpenSage.Tests.Terrain.Roads
{
    public class CurveTypeTests
    {
        public enum NodePosition
        {
            NW,
            NE,
            SW,
            SE
        }

        private static IReadOnlyDictionary<NodePosition, Vector3> NodePositions = new Dictionary<NodePosition, Vector3>()
        {
            [NW] = new Vector3(100, 200, 0),
            [NE] = new Vector3(200, 200, 0),
            [SW] = new Vector3(100, 100, 0),
            [SE] = new Vector3(200, 100, 0)
        };

        [Theory]
        [InlineData(SW, Angled, NW, Angled, SW, TightCurve, SE, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, Angled, NW, TightCurve, NE, TightCurve, RoadTextureType.TightCurve)]
        [InlineData(SW, Angled, NW, Angled, SE, TightCurve, SW, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, Angled, NE, TightCurve, NW, TightCurve, RoadTextureType.TightCurve)]

        [InlineData(SW, TightCurve, NW, TightCurve, SW, Angled, SE, Angled, RoadTextureType.TightCurve)]
        [InlineData(SW, TightCurve, NW, TightCurve, NW, Angled, NE, Angled, RoadTextureType.Straight)]
        [InlineData(SW, TightCurve, NW, TightCurve, SE, Angled, SW, Angled, RoadTextureType.TightCurve)]
        [InlineData(SW, TightCurve, NW, TightCurve, NE, Angled, NW, Angled, RoadTextureType.Straight)]

        [InlineData(SW, TightCurve, NW, TightCurve, SW, Angled, SE, TightCurve, RoadTextureType.TightCurve)]
        [InlineData(SW, TightCurve, NW, TightCurve, NW, Angled, NE, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, TightCurve, NW, TightCurve, SE, Angled, SW, TightCurve, RoadTextureType.TightCurve)]
        [InlineData(SW, TightCurve, NW, TightCurve, NE, Angled, NW, TightCurve, RoadTextureType.BroadCurve)]

        [InlineData(SW, Angled, NW, TightCurve, SW, TightCurve, SE, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, TightCurve, NW, TightCurve, NE, TightCurve, RoadTextureType.TightCurve)]
        [InlineData(SW, Angled, NW, TightCurve, SE, TightCurve, SW, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, TightCurve, NE, TightCurve, NW, TightCurve, RoadTextureType.TightCurve)]

        [InlineData(SW, Angled, NW, TightCurve, SW, Angled, SE, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, TightCurve, NW, Angled, NE, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, TightCurve, SE, Angled, SW, TightCurve, RoadTextureType.Straight)]
        [InlineData(SW, Angled, NW, TightCurve, NE, Angled, NW, TightCurve, RoadTextureType.BroadCurve)]
        public void CurveType(
            NodePosition startPosition1, RoadType startType1,
            NodePosition endPosition1, RoadType endType1,
            NodePosition startPosition2, RoadType startType2,
            NodePosition endPosition2, RoadType endType2,
            RoadTextureType expectedCurveType)
        {
            var start1 = new MapObject(NodePositions[startPosition1], 0, startType1, default);
            var end1 = new MapObject(NodePositions[endPosition1], 0, endType1, default);
            var start2 = new MapObject(NodePositions[startPosition2], 0, startType2, default);
            var end2 = new MapObject(NodePositions[endPosition2], 0, endType2, default);

            var template = new RoadTemplate("SideWalk3");

            var topology = new RoadTopology();
            topology.AddSegment(template, start1, end1);
            topology.AddSegment(template, start2, end2);

            topology.AlignOrientation();

            var curveNode = topology.Nodes.Single(n => n.Edges.Count == 2);
            var actualCurveType = CurvedRoadSegment.ChooseCurveType(topology.Edges[0], topology.Edges[1], curveNode.Position);

            Assert.Equal(expectedCurveType, actualCurveType);
        }
    }
}
