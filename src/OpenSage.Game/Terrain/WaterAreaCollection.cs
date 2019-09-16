using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Terrain
{
    public sealed class WaterAreaCollection : DisposableBase
    {
        private readonly List<WaterArea> _waterAreas;

        public WaterAreaCollection()
        {
            _waterAreas = new List<WaterArea>();
        }

        internal WaterAreaCollection(PolygonTriggers polygonTriggers, AssetLoadContext loadContext)
            : this()
        {
            if (polygonTriggers != null)
            {
                foreach (var polygonTrigger in polygonTriggers.Triggers)
                {
                    switch (polygonTrigger.TriggerType)
                    {
                        case PolygonTriggerType.Water:
                        case PolygonTriggerType.River: // TODO: Handle this differently. Water texture should be animated "downstream".
                        case PolygonTriggerType.WaterAndRiver:
                            if (WaterArea.TryCreate(loadContext, polygonTrigger, out var waterArea))
                            {
                                _waterAreas.Add(AddDisposable(waterArea));
                            }
                            break;
                    }
                }
            }
        }

        internal void BuildRenderList(RenderList renderList)
        {
            foreach (var waterArea in _waterAreas)
            {
                waterArea.BuildRenderList(renderList);
            }
        }
    }
}
