using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Terrain.Roads
{
    public sealed class RoadCollection : DisposableBase, IReadOnlyList<Road>
    {
        private readonly List<Road> _roads;

        internal RoadCollection()
        {
            _roads = new List<Road>();
        }

        internal RoadCollection(
            RoadTopology topology,
            AssetLoadContext loadContext,
            HeightMap heightMap)
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

            var roadTemplateList = new RoadTemplateList(loadContext.AssetStore.RoadTemplates);

            var networks = RoadNetwork.BuildNetworks(topology, roadTemplateList);

            foreach (var network in networks)
            {
                _roads.Add(AddDisposable(new Road(
                        loadContext,
                        heightMap,
                        network)));
            }
        }

        public Road this[int index] => _roads[index];

        public int Count => _roads.Count;

        public IEnumerator<Road> GetEnumerator() => _roads.GetEnumerator();

        internal void BuildRenderList(RenderList renderList)
        {
            foreach (var road in _roads)
            {
                road.BuildRenderList(renderList);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
