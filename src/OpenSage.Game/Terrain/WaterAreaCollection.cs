using System.Collections.Generic;
using OpenSage.Content;
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

        public WaterAreaCollection(PolygonTriggers polygonTriggers, ContentManager contentManager)
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
                            if (WaterArea.TryCreate(contentManager, polygonTrigger, out var waterArea))
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
