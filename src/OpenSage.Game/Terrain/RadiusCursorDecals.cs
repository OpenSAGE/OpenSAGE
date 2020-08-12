using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.InGame;
using Veldrid;

namespace OpenSage.Terrain
{
    internal sealed class RadiusCursorDecals : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ConstantBuffer<RadiusCursorDecalShaderResources.RadiusCursorDecalConstants> _decalConstantBuffer;

        private readonly Dictionary<string, uint> _nameToTextureIndex;

        private readonly List<DecalHandle> _decalsStorage;

        private const uint MaxDecals = 8;
        private readonly RadiusCursorDecal[] _decals;

        public readonly DeviceBuffer DecalConstants;

        public readonly DeviceBuffer DecalsBuffer;

        public readonly Texture TextureArray;

        public RadiusCursorDecals(AssetStore assetStore, GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            _decalConstantBuffer = AddDisposable(new ConstantBuffer<RadiusCursorDecalShaderResources.RadiusCursorDecalConstants>(
                graphicsDevice,
                "RadiusCursorDecalConstants"));

            DecalConstants = _decalConstantBuffer.Buffer;

            DecalsBuffer = AddDisposable(graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    RadiusCursorDecal.SizeInBytes * MaxDecals,
                    BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic,
                    RadiusCursorDecal.SizeInBytes,
                    true)));

            _decalsStorage = new List<DecalHandle>();

            _decals = new RadiusCursorDecal[MaxDecals];

            _nameToTextureIndex = new Dictionary<string, uint>();

            TextureArray = CreateTextureArray(assetStore, graphicsDevice);
        }

        private Texture CreateTextureArray(AssetStore assetStore, GraphicsDevice graphicsDevice)
        {
            var textures = new List<Texture>();

            // TODO: Radius cursors don't only come from InGameUI. They can also come from other sources
            // like ObjectCreationList.DeliverPayload.DeliveryDecal.

            var largestTextureSize = uint.MinValue;
            foreach (var radiusCursor in assetStore.InGameUI.Current.RadiusCursors)
            {
                var texture = radiusCursor.Value.DecalTemplate.Texture.Value.Texture;

                if (texture.Width != texture.Height)
                {
                    throw new InvalidOperationException();
                }

                if (texture.Width > largestTextureSize)
                {
                    largestTextureSize = texture.Width;
                }

                // TODO: Total hack, to avoid handling different-sized textures.
                // Seems only 512px textures are actually used in Generals.
                if (texture.Width != 512)
                {
                    continue;
                }

                var textureIndex = textures.Count;

                _nameToTextureIndex.Add(radiusCursor.Key, (uint)textureIndex);

                textures.Add(texture);
            }

            var mipLevels = textures[0].MipLevels;

            var textureArray = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    largestTextureSize,
                    largestTextureSize,
                    mipLevels,
                    (uint) textures.Count,
                    textures[0].Format, // TODO
                    TextureUsage.Sampled)));

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            commandList.Begin();

            for (var i = 0u; i < textures.Count; i++)
            {
                var sourceTexture = textures[(int) i];

                for (var mipLevel = 0u; mipLevel < mipLevels; mipLevel++)
                {
                    var mipSize = CalculateMipSize(mipLevel, largestTextureSize);

                    commandList.CopyTexture(
                        sourceTexture,
                        0, 0, 0,
                        mipLevel,
                        0,
                        textureArray,
                        0, 0, 0,
                        mipLevel,
                        i,
                        mipSize,
                        mipSize,
                        1,
                        1);
                }
            }

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            graphicsDevice.DisposeWhenIdle(commandList);

            graphicsDevice.WaitForIdle();

            return textureArray;
        }

        private static uint CalculateMipSize(uint mipLevel, uint baseSize)
        {
            baseSize >>= (int)mipLevel;
            return baseSize > 0 ? baseSize : 1;
        }

        public void Update(in TimeInterval time)
        {
            _decalConstantBuffer.Value.NumRadiusCursorDecals = (uint)_decalsStorage.Count;
            _decalConstantBuffer.Update(_graphicsDevice);

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
                        int j = 0;
                    }
                }
                else
                {
                    handle.Decal.Opacity -= opacityDelta;
                }

                _decals[i] = handle.Decal;
            }

            _graphicsDevice.UpdateBuffer(DecalsBuffer, 0, _decals);
        }

        public DecalHandle AddDecal(
            string radiusCursorName,
            RadiusDecalTemplate template,
            float radius,
            in TimeInterval time)
        {
            if (_decalsStorage.Count == MaxDecals)
            {
                _decalsStorage.RemoveAt(0);
            }

            DecalHandle result;
            _decalsStorage.Add(result = new DecalHandle
            {
                Decal = new RadiusCursorDecal
                {
                    DecalTextureIndex = _nameToTextureIndex[radiusCursorName],
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
