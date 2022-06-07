﻿using System.Collections;
using System.Collections.Generic;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;

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
            HeightMap heightMap,
            RenderScene scene)
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

            var renderBucket = scene.CreateRenderBucket("Roads", 10);

            foreach (var network in networks)
            {
                var road = AddDisposable(
                    new Road(
                        loadContext,
                        heightMap,
                        network));

                renderBucket.AddObject(road);

                _roads.Add(road);
            }
        }

        public Road this[int index] => _roads[index];

        public int Count => _roads.Count;

        public IEnumerator<Road> GetEnumerator() => _roads.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
