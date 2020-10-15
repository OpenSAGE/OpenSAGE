using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.InGame;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Terrain
{
    internal sealed class RadiusCursorDecals : DisposableBase
    {
        private readonly AssetStore _assetStore;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly ConstantBuffer<RadiusCursorDecalShaderResources.RadiusCursorDecalConstants> _decalConstantBuffer;

        private readonly Dictionary<Texture, uint> _nameToTextureIndex;

        private readonly List<DecalHandle> _decalsStorage;

        private const uint MaxDecals = 8;
        private readonly RadiusCursorDecal[] _decals;

        // TODO: Support non-512px texture sizes.
        private const uint TextureSize = 512;
        private static readonly uint TextureMipLevels = TextureMipMapData.CalculateMipMapCount(TextureSize, TextureSize);

        private const uint MaxTextures = 20; // This can be increased if necessary.
        private uint _nextTextureIndex;

        public readonly DeviceBuffer DecalConstants;

        public readonly DeviceBuffer DecalsBuffer;

        public readonly Texture TextureArray;

        public RadiusCursorDecals(AssetStore assetStore, GraphicsDevice graphicsDevice)
        {
            _assetStore = assetStore;
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

            _nameToTextureIndex = new Dictionary<Texture, uint>();

            TextureArray = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    TextureSize,
                    TextureSize,
                    TextureMipLevels,
                    MaxTextures,
                    PixelFormat.BC3_UNorm, // TODO: Allow other types
                    TextureUsage.Sampled)));
        }

        private uint GetTextureIndex(Texture texture)
        {
            if (!_nameToTextureIndex.TryGetValue(texture, out var result))
            {
                if (_nextTextureIndex == MaxTextures ||
                    texture.Width != texture.Height ||
                    texture.Width != TextureSize)
                {
                    throw new InvalidOperationException();
                }

                result = _nextTextureIndex;

                _nameToTextureIndex.Add(texture, _nextTextureIndex);

                var commandList = _graphicsDevice.ResourceFactory.CreateCommandList();
                commandList.Begin();

                for (var mipLevel = 0u; mipLevel < TextureMipLevels; mipLevel++)
                {
                    var mipSize = TextureMipMapData.CalculateMipSize(mipLevel, TextureSize);

                    commandList.CopyTexture(
                        texture,
                        0, 0, 0,
                        mipLevel,
                        0,
                        TextureArray,
                        0, 0, 0,
                        mipLevel,
                        _nextTextureIndex,
                        mipSize,
                        mipSize,
                        1,
                        1);
                }

                commandList.End();

                _graphicsDevice.SubmitCommands(commandList);

                _graphicsDevice.DisposeWhenIdle(commandList);

                _graphicsDevice.WaitForIdle();

                _nextTextureIndex++;
            }

            return result;
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
                        // TODO:
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
            RadiusDecalTemplate template,
            float radius,
            in TimeInterval time)
        {
            if (_decalsStorage.Count == MaxDecals)
            {
                _decalsStorage.RemoveAt(0);
            }

            var textureIndex = GetTextureIndex(template.Texture.Value.Texture);

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
