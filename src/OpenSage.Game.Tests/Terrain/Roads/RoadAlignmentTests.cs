using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Terrain.Roads;
using Xunit;
using static OpenSage.Terrain.Roads.RoadTextureType;

namespace OpenSage.Tests.Terrain.Roads
{
    public class RoadAlignmentTests
    {
        [Fact]
        public void NoAlignmentChange()
        {
            var topology = LoadTopologyFromMapFile();

            AssertCurveType(topology.Nodes[0], BroadCurve);
            AssertCurveType(topology.Nodes[1], Straight);
            AssertCurveType(topology.Nodes[2], Straight);
            AssertCurveType(topology.Nodes[3], TightCurve);
        }

        [Fact]
        public void AlignmentChange()
        {
            var topology = LoadTopologyFromMapFile();

            AssertCurveType(topology.Nodes[0], Straight);
            AssertCurveType(topology.Nodes[1], BroadCurve);
            AssertCurveType(topology.Nodes[2], Straight);
            AssertCurveType(topology.Nodes[3], BroadCurve);
        }

        [Fact]
        public void CHI01()
        {
            var topology = LoadTopologyFromMapFile();

            AssertCurveType(topology.Nodes[0], Straight);
            AssertCurveType(topology.Nodes[1], BroadCurve);
            AssertCurveType(topology.Nodes[3], Straight);
        }

        private RoadTopology LoadTopologyFromMapFile([CallerMemberName]string mapName = "")
        {
            var fileSystem = new FileSystem(Path.Combine(Environment.CurrentDirectory, "Terrain", "Roads", "RoadAlignmentTests"));
            var mapFile = MapFile.FromFileSystemEntry(fileSystem.GetFile(mapName + ".map"));

            var topology = RoadTopologyLoader.FromMapObjects(mapFile.ObjectsList.Objects);
            topology.AlignOrientation();

            return topology;
        }

        private void AssertCurveType(RoadTopologyNode node, RoadTextureType expectedCurveType)
        {
            var actualCurveType = CurvedRoadSegment.ChooseCurveType(node.Edges[0], node.Edges[1], node.Position);
            Assert.Equal(expectedCurveType, actualCurveType);
        }
    }
}
