using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenZH.Data;
using OpenZH.Data.W3d;
using OpenZH.Game.Graphics;
using OpenZH.Game.Util;
using OpenZH.Graphics;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public sealed class W3dMeshView : RenderedView
    {
        private Mesh _mesh;

        private MeshTransformConstants _meshTransformConstants;
        private DynamicBuffer _meshTransformConstantBuffer;

        // TODO: Make this dynamic, based on mesh size.
        private readonly Vector3 _cameraPosition = new Vector3(0, 1, 30);

        private PipelineLayout _pipelineLayout;
        private DescriptorSet _descriptorSetPixel;
        private PipelineState _pipelineState;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _lastUpdate;

        public FileSystem FileSystem { get; set; }

        public static readonly BindableProperty MeshProperty = BindableProperty.Create(
            nameof(Mesh), typeof(W3dMesh), typeof(W3dMeshView));

        public W3dMesh Mesh
        {
            get { return (W3dMesh) GetValue(MeshProperty); }
            set { SetValue(MeshProperty, value); }
        }

        public W3dMeshView()
        {
            RedrawsOnTimer = true;
        }

        public override void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            _mesh = new Mesh(Mesh);
            _mesh.Initialize(graphicsDevice, uploadBatch);

            var numTextures = Mesh.Textures.Length;

            var descriptorSetLayoutPixel = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    new DescriptorSetLayoutBinding(DescriptorType.ConstantBuffer, 0, 1),
                    new DescriptorSetLayoutBinding(DescriptorType.ConstantBuffer, 1, 1),
                    new DescriptorSetLayoutBinding(DescriptorType.TypedBuffer, 0, 1),
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 1, numTextures)
                }
            });

            _pipelineLayout = new PipelineLayout(graphicsDevice, new PipelineLayoutDescription
            {
                InlineDescriptorLayouts = new[]
                {
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Vertex,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    }
                },
                DescriptorSetLayouts = new[]
                {
                    descriptorSetLayoutPixel
                },
                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        ShaderRegister = 0,
                        SamplerStateDescription = new SamplerStateDescription
                        {
                            Filter = SamplerFilter.Anisotropic
                        }
                    }
                }
            });

            _meshTransformConstantBuffer = DynamicBuffer.Create<MeshTransformConstants>(graphicsDevice);

            _descriptorSetPixel = new DescriptorSet(graphicsDevice, descriptorSetLayoutPixel);

            var lightingConstantBuffer = StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                new LightingConstants
                {
                    CameraPosition = _cameraPosition,
                    AmbientLightColor = new Vector3(0.3f, 0.3f, 0.3f),
                    Light0Direction = Vector3.Normalize(new Vector3(-0.3f, -0.8f, -0.2f)),
                    Light0Color = new Vector3(0.5f, 0.5f, 0.5f)
                });

            _descriptorSetPixel.SetConstantBuffer(0, lightingConstantBuffer);

            // TODO
            var vertexMaterial = Mesh.Materials[0].VertexMaterialInfo;

            var materialConstantBuffer = StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                new MaterialConstants
                {
                    MaterialAmbient = vertexMaterial.Ambient.ToVector3(),
                    MaterialDiffuse = vertexMaterial.Diffuse.ToVector3(),
                    MaterialSpecular = vertexMaterial.Specular.ToVector3(),
                    MaterialEmissive = vertexMaterial.Emissive.ToVector3(),
                    MaterialShininess = vertexMaterial.Shininess,
                    MaterialOpacity = vertexMaterial.Opacity
                });

            _descriptorSetPixel.SetConstantBuffer(1, materialConstantBuffer);

            var textureIds = Mesh.MaterialPasses[0].TextureStages[0].TextureIds;
            if (textureIds.Length == 1)
            {
                var textureId = textureIds[0];
                textureIds = new uint[Mesh.Header.NumTris];
                for (var i = 0; i < Mesh.Header.NumTris; i++)
                {
                    textureIds[i] = textureId;
                }
            }

            var textureIndicesBuffer = StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                textureIds);

            _descriptorSetPixel.SetTypedBuffer(2, textureIndicesBuffer, PixelFormat.UInt32);

            var textures = new Texture[numTextures];
            for (var i = 0; i < numTextures; i++)
            {
                var w3dTexture = Mesh.Textures[i];
                var textureName = w3dTexture.Name.Replace(".tga", ".dds"); // Always right?
                var texturePath = $@"Art\Textures\{textureName}";

                var textureFileSystemEntry = FileSystem.GetFile(texturePath);

                textures[i] = TextureLoader.LoadTexture(
                    graphicsDevice,
                    uploadBatch,
                    textureFileSystemEntry);
            }

            _descriptorSetPixel.SetTextures(3, textures);

            uploadBatch.End();

            var shaderLibrary = new ShaderLibrary(graphicsDevice);

            var pixelShader = new Shader(shaderLibrary, "MeshPS");
            var vertexShader = new Shader(shaderLibrary, "MeshVS");

            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "TEXCOORD", 0, VertexFormat.Float2, 0, 24);
            vertexDescriptor.SetLayoutDescriptor(0, 32);

            _pipelineState = new PipelineState(graphicsDevice, new PipelineStateDescription
            {
                PipelineLayout = _pipelineLayout,
                PixelShader = pixelShader,
                RenderTargetFormat = swapChain.BackBufferFormat,
                VertexDescriptor = vertexDescriptor,
                VertexShader = vertexShader
            });

            _stopwatch.Start();
            _lastUpdate = 0;
        }

        private void Update()
        {
            var now = _stopwatch.ElapsedMilliseconds * 0.001;
            var updateTime = now - _lastUpdate;
            _lastUpdate = now;

            var world = Matrix4x4.CreateRotationY((float) _lastUpdate);

            var view = Matrix4x4.CreateLookAt(
                _cameraPosition,
                Vector3.Zero,
                Vector3.UnitY);

            var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                (float) (90 * System.Math.PI / 180),
                (float) (Width / Height),
                0.1f,
                100.0f);

            _meshTransformConstants.WorldViewProjection = world * view * projection;
            _meshTransformConstants.World = world;
            _meshTransformConstantBuffer.SetData(ref _meshTransformConstants);
        }

        public override void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            Update();

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetPipelineState(_pipelineState);

            commandEncoder.SetPipelineLayout(_pipelineLayout);

            commandEncoder.SetInlineConstantBuffer(0, _meshTransformConstantBuffer);

            commandEncoder.SetDescriptorSet(1, _descriptorSetPixel);

            commandEncoder.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = (int) Width,
                Height = (int) Height,
                MinDepth = 0,
                MaxDepth = 1
            });

            _mesh.Draw(commandEncoder);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
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

        [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
        public struct MaterialConstants
        {
            public const int SizeInBytes = 64;

            [FieldOffset(0)]
            public Vector3 MaterialAmbient;

            [FieldOffset(16)]
            public Vector3 MaterialDiffuse;

            [FieldOffset(32)]
            public Vector3 MaterialSpecular;

            [FieldOffset(44)]
            public float MaterialShininess;

            [FieldOffset(48)]
            public Vector3 MaterialEmissive;

            [FieldOffset(60)]
            public float MaterialOpacity;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshTransformConstants
        {
            public const int SizeInBytes = 128;

            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
        }
    }
}
