using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class Map : GraphicsObject
    {
        private readonly int _numPatchesX, _numPatchesY;
        private readonly TerrainPatch[,] _patches;

        private readonly PipelineLayout _pipelineLayout;
        private readonly PipelineState _pipelineState;

        private readonly DynamicBuffer _transformConstantBuffer;
        private TransformConstants _transformConstants;

        private readonly DynamicBuffer _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        public Map(MapFile mapFile, GraphicsDevice graphicsDevice)
        {
            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            const int patchSize = 17;
            const int numTilesPerPatch = patchSize - 1;

            var heightMap = new HeightMap(mapFile.HeightMapData);

            _numPatchesX = heightMap.Width / numTilesPerPatch;
            if (heightMap.Width % numTilesPerPatch != 0)
            {
                _numPatchesX += 1;
            }

            _numPatchesY = heightMap.Height / numTilesPerPatch;
            if (heightMap.Height % numTilesPerPatch != 0)
            {
                _numPatchesY += 1;
            }

            _patches = new TerrainPatch[_numPatchesX, _numPatchesY];

            for (var y = 0; y < _numPatchesY; y++)
            {
                for (var x = 0; x < _numPatchesX; x++)
                {
                    var patchX = x * numTilesPerPatch;
                    var patchY = y * numTilesPerPatch;

                    var patchBounds = new Int32Rect
                    {
                        X = patchX,
                        Y = patchY,
                        Width = System.Math.Min(patchSize, heightMap.Width - patchX),
                        Height = System.Math.Min(patchSize, heightMap.Height - patchY)
                    };

                    _patches[x, y] = AddDisposable(new TerrainPatch(
                        heightMap,
                        patchBounds,
                        graphicsDevice,
                        uploadBatch));
                }
            }

            uploadBatch.End();

            _pipelineLayout = AddDisposable(new PipelineLayout(graphicsDevice, new PipelineLayoutDescription
            {
                InlineDescriptorLayouts = new [] 
                {
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Vertex,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },

                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    },
                },
                DescriptorSetLayouts = new DescriptorSetLayout[] { },
                StaticSamplerStates = new StaticSamplerDescription[] { }
            }));

            var shaderLibrary = AddDisposable(new ShaderLibrary(graphicsDevice));
            var vertexShader = AddDisposable(new Shader(shaderLibrary, "TerrainVS"));
            var pixelShader = AddDisposable(new Shader(shaderLibrary, "TerrainPS"));

            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetLayoutDescriptor(0, 24);

            var pipelineStateDescription = PipelineStateDescription.Default();
            pipelineStateDescription.PipelineLayout = _pipelineLayout;
            pipelineStateDescription.RenderTargetFormat = graphicsDevice.BackBufferFormat;
            pipelineStateDescription.VertexDescriptor = vertexDescriptor;
            pipelineStateDescription.VertexShader = vertexShader;
            pipelineStateDescription.PixelShader = pixelShader;

            pipelineStateDescription.TwoSided = true; // TODO: Remove this.

            _pipelineState = AddDisposable(new PipelineState(graphicsDevice, pipelineStateDescription));

            _transformConstantBuffer = AddDisposable(DynamicBuffer.Create<TransformConstants>(graphicsDevice));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer.Create<LightingConstants>(graphicsDevice));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        private struct LightingConstants
        {
            public const int SizeInBytes = 64;

            [FieldOffset(0)]
            public Vector3 CameraPosition;

            [FieldOffset(16)]
            public Vector3 AmbientLightColor;

            [FieldOffset(32)]
            public Vector3 Light0Direction;

            [FieldOffset(48)]
            public Vector3 Light0Color;
        }

        public void Draw(CommandEncoder commandEncoder,
            ref Vector3 cameraPosition,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            commandEncoder.SetPipelineLayout(_pipelineLayout);

            commandEncoder.SetPipelineState(_pipelineState);

            _transformConstants.WorldViewProjection = view * projection;
            _transformConstants.World = Matrix4x4.Identity;
            _transformConstantBuffer.SetData(ref _transformConstants);
            commandEncoder.SetInlineConstantBuffer(0, _transformConstantBuffer);

            _lightingConstants.CameraPosition = cameraPosition;
            _lightingConstants.AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f);
            _lightingConstants.Light0Direction = Vector3.Normalize(new Vector3(-0.3f, -0.8f, -0.2f));
            _lightingConstants.Light0Color = new Vector3(0.7f, 0.7f, 0.8f);
            _lightingConstantBuffer.SetData(ref _lightingConstants);
            commandEncoder.SetInlineConstantBuffer(1, _lightingConstantBuffer);

            for (var y = 0; y < _numPatchesY; y++)
            {
                for (var x = 0; x < _numPatchesX; x++)
                {
                    // TODO: Frustum culling.
                    _patches[x, y].Draw(commandEncoder);
                }
            }
        }
    }
}
