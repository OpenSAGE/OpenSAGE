using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Terrain
{
    public sealed class RoadCollection : DisposableBase
    {
        private readonly List<Road> _roads;

        internal RoadCollection()
        {
            _roads = new List<Road>();
        }

        internal RoadCollection(RoadTopology topology, AssetLoadContext loadContext, HeightMap heightMap)
            : this()
        {
            // The map stores road segments with no connectivity:
            // - a segment is from point A to point B
            // - with a road type name
            // - and start and end curve types (angled, tight curve, broad curve).

            // The goal is to create road networks of connected road segments,
            // where a network has only a single road type.

            // A road network is composed of 2 or more nodes.
            // A network is a (potentially) cyclic graph.

            // A road node has > 1 and <= 4 edges connected to it.
            // A node can be part of multiple networks.

            // An edge can only exist in one network.

            // TODO: If a node stored in the map has > 4 edges, the extra edges
            // are put into a separate network.

            var networks = topology.BuildNetworks();

            foreach (var network in networks)
            {
                foreach (var edge in network.Edges)
                {
                    var startPosition = edge.Start.TopologyNode.Position;
                    var endPosition = edge.End.TopologyNode.Position;

                    startPosition.Z += heightMap.GetHeight(startPosition.X, startPosition.Y);
                    endPosition.Z += heightMap.GetHeight(endPosition.X, endPosition.Y);

                    _roads.Add(AddDisposable(new Road(
                        loadContext,
                        heightMap,
                        edge.TopologyEdge.Template,
                        startPosition,
                        endPosition)));
                }
            }
        }

        internal void BuildRenderList(RenderList renderList)
        {
            foreach (var road in _roads)
            {
                road.BuildRenderList(renderList);
            }
        }
    }
}
