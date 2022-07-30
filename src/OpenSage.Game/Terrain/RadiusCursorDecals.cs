using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.InGame;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Terrain
{
    internal sealed class RadiusCursorDecals : DisposableBase
    {
        private readonly RadiusCursorDecalResourceData _data;

        private readonly List<DecalHandle> _decalsStorage;

        public RadiusCursorDecals(RadiusCursorDecalResourceData data)
        {
            _data = data;

            _decalsStorage = new List<DecalHandle>();
        }

        public void Update(in TimeInterval time)
        {
            _data.SetNumRadiusCursorDecals((uint)_decalsStorage.Count);

            for (var i = 0; i < _decalsStorage.Count; i++)
            {
                var handle = _decalsStorage[i];

                var opacityDelta = (float)(handle.OpacityDeltaPerMillisecond * time.DeltaTime.TotalMilliseconds);
                if (time.TotalTime > handle.NextOpacityDirectionChange)
                {
                    handle.IsOpacityIncreasing = !handle.IsOpacityIncreasing;
                    handle.NextOpacityDirectionChange = time.TotalTime + handle.Template.OpacityThrobTime / 2f;
                }
                if (handle.IsOpacityIncreasing)
                {
                    handle.Decal.Opacity += opacityDelta;
                    if (handle.Decal.Opacity > 1)
                    {
                        // TODO:
                    }
                }
                else
                {
                    handle.Decal.Opacity -= opacityDelta;
                }

                _data.SetDecal(i, handle.Decal);
            }

            _data.UpdateDecalsBuffer();
        }

        public DecalHandle AddDecal(
            RadiusDecalTemplate template,
            float radius,
            in TimeInterval time)
        {
            if (_decalsStorage.Count == RadiusCursorDecalResourceData.MaxDecals)
            {
                _decalsStorage.RemoveAt(0);
            }

            var textureIndex = _data.GetTextureIndex(template.Texture.Value.Texture);

            DecalHandle result;
            _decalsStorage.Add(result = new DecalHandle
            {
                Decal = new RadiusCursorDecal
                {
                    DecalTextureIndex = textureIndex,
                    Diameter = radius * 2,
                    Opacity = (float) template.OpacityMin,
                },
                Template = template,
                IsOpacityIncreasing = true,
                OpacityDeltaPerMillisecond = (float)(((float)template.OpacityMax - (float)template.OpacityMin) / template.OpacityThrobTime.TotalMilliseconds),
                NextOpacityDirectionChange = time.TotalTime + template.OpacityThrobTime / 2f,
            });

            return result;
        }

        public void SetDecalPosition(DecalHandle handle, Vector2 position)
        {
            var radius = handle.Decal.Diameter / 2.0f;
            handle.Decal.BottomLeftCornerPosition = position - new Vector2(radius, radius);
        }

        public void RemoveDecal(DecalHandle handle)
        {
            _decalsStorage.Remove(handle);
        }
    }

    internal sealed class DecalHandle
    {
        public RadiusCursorDecal Decal;
        public RadiusDecalTemplate Template;
        public bool IsOpacityIncreasing;
        public float OpacityDeltaPerMillisecond;
        public TimeSpan NextOpacityDirectionChange;
    }
}
