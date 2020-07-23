using OpenSage.Terrain.Roads;
using Xunit;

namespace OpenSage.Tests.Terrain.Roads
{
    /// <summary>
    /// These tests verify that the road template orderings works as in the
    /// original engine. They are based on observations made in Worldbuilder
    /// (see the attached map files).
    ///
    /// The order in which the roads are created is important!
    ///
    /// Each call to <see cref="RoadTemplateList.HandleRoadJoin"/> corresponds
    /// to one click on the "Add end cap and/or Join to different road" checkbox
    /// for the given road, which changes the overall road template ordering.
    /// </summary>
    public class RoadTemplateListTests
    {
        [Fact]
        public void DarkOldDirtFourGravel()
        {
            var gravel = new RoadTemplate("GravelRoad");
            var dirt = new RoadTemplate("DirtRoad");
            var four = new RoadTemplate("FourLane");
            var old = new RoadTemplate("TwoLaneOld");
            var dark = new RoadTemplate("TwoLaneDark");

            var templates = new[]
            {
                dark,
                old,
                four,
                dirt,
                gravel,
            };

            var roadTemplateList = new RoadTemplateList(templates);

            roadTemplateList.HandleRoadJoin(dark, old);

            var expectedOrder = new[]
            {
                gravel,
                dirt,
                four,
                old,
                dark,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(old, dirt);

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(dirt, four);

            expectedOrder = new[]
            {
                gravel,
                four,
                dirt,
                old,
                dark,
            };

            roadTemplateList.HandleRoadJoin(four, gravel);

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(gravel, dark);

            Assert.Equal(expectedOrder, roadTemplateList);
        }

        [Fact]
        public void GravelFourDirtOldDark()
        {
            var gravel = new RoadTemplate("GravelRoad");
            var dirt = new RoadTemplate("DirtRoad");
            var four = new RoadTemplate("FourLane");
            var old = new RoadTemplate("TwoLaneOld");
            var dark = new RoadTemplate("TwoLaneDark");

            var templates = new[]
            {
                dark,
                old,
                four,
                dirt,
                gravel,
            };

            var roadTemplateList = new RoadTemplateList(templates);

            roadTemplateList.HandleRoadJoin(gravel, dark);

            var expectedOrder = new[]
            {
                dirt,
                four,
                old,
                dark,
                gravel,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(four, gravel);

            expectedOrder = new[]
            {
                dirt,
                old,
                dark,
                gravel,
                four,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(dirt, four);

            expectedOrder = new[]
            {
                old,
                dark,
                gravel,
                four,
                dirt,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(old, dirt);

            expectedOrder = new[]
            {
                dark,
                gravel,
                four,
                dirt,
                old,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(dark, old);

            expectedOrder = new[]
            {
                four,
                dirt,
                old,
                dark,
                gravel,
            };

            Assert.Equal(expectedOrder, roadTemplateList);
        }

        [Fact]
        public void FourOldGravelDirtDark()
        {
            var gravel = new RoadTemplate("GravelRoad");
            var dirt = new RoadTemplate("DirtRoad");
            var four = new RoadTemplate("FourLane");
            var old = new RoadTemplate("TwoLaneOld");
            var dark = new RoadTemplate("TwoLaneDark");

            var templates = new[]
            {
                dark,
                old,
                four,
                dirt,
                gravel,
            };

            var roadTemplateList = new RoadTemplateList(templates);

            roadTemplateList.HandleRoadJoin(four, gravel);

            var expectedOrder = new[]
            {
                gravel,
                dirt,
                old,
                dark,
                four,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(old, dirt);

            expectedOrder = new[]
            {
                gravel,
                dirt,
                dark,
                four,
                old,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(gravel, dark);

            expectedOrder = new[]
            {
                dirt,
                dark,
                old,
                gravel,
                four,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(dirt, four);

            expectedOrder = new[]
            {
                dark,
                gravel,
                four,
                dirt,
                old,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(dark, old);

            expectedOrder = new[]
            {
                dirt,
                old,
                dark,
                gravel,
                four,
            };

            Assert.Equal(expectedOrder, roadTemplateList);
        }

        [Fact]
        public void DarkDirtGravelOldFour()
        {
            var gravel = new RoadTemplate("GravelRoad");
            var dirt = new RoadTemplate("DirtRoad");
            var four = new RoadTemplate("FourLane");
            var old = new RoadTemplate("TwoLaneOld");
            var dark = new RoadTemplate("TwoLaneDark");

            var templates = new[]
            {
                dark,
                old,
                four,
                dirt,
                gravel,
            };

            var roadTemplateList = new RoadTemplateList(templates);

            roadTemplateList.HandleRoadJoin(dark, old);

            var expectedOrder = new[]
            {
                gravel,
                dirt,
                four,
                old,
                dark,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(dirt, four);

            expectedOrder = new[]
            {
                gravel,
                four,
                old,
                dark,
                dirt,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(gravel, dark);

            expectedOrder = new[]
            {
                four,
                old,
                dark,
                dirt,
                gravel,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(old, dirt);

            expectedOrder = new[]
            {
                four,
                dirt,
                gravel,
                old,
                dark,
            };

            Assert.Equal(expectedOrder, roadTemplateList);

            roadTemplateList.HandleRoadJoin(four, gravel);

            expectedOrder = new[]
            {
                gravel,
                old,
                dark,
                four,
                dirt,
            };

            Assert.Equal(expectedOrder, roadTemplateList);
        }
    }
}
