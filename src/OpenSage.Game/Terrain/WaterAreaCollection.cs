using System.Collections.Generic;
using OpenSage.Core.Graphics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering;
using OpenSage.Rendering;

namespace OpenSage.Terrain
{
    public sealed class WaterAreaCollection : DisposableBase
    {
        private readonly List<WaterArea> _waterAreas;

        public WaterAreaCollection()
        {
            _waterAreas = new List<WaterArea>();
        }

        internal WaterAreaCollection(
            PolygonTriggers polygonTriggers,
            StandingWaterAreas standingWaterAreas,
            StandingWaveAreas standingWaveAreas,
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore)
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
                            if (WaterArea.TryCreate(graphicsDeviceManager, shaderSetStore, polygonTrigger, out var waterArea))
                            {
                                _waterAreas.Add(AddDisposable(waterArea));
                            }
                            break;
                    }
                }
            }

            if (standingWaterAreas != null)
            {
                foreach (var standingWaterArea in standingWaterAreas.Areas)
                {
                    if (WaterArea.TryCreate(graphicsDeviceManager, shaderSetStore, standingWaterArea, out var waterArea))
                    {
                        _waterAreas.Add(AddDisposable(waterArea));
                    }
                }
            }

            if (standingWaveAreas != null)
            {
                foreach (var standingWaveArea in standingWaveAreas.Areas)
                {
                    if (WaterArea.TryCreate(graphicsDeviceManager, shaderSetStore, standingWaveArea, out var waterArea))
                    {
                        _waterAreas.Add(AddDisposable(waterArea));
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
