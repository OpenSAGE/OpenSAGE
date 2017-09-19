using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class ModelMesh : GraphicsObject
    {
        private readonly uint _numVertices;
        private readonly StaticBuffer<MeshVertex> _vertexBuffer;

        private readonly uint _numIndices;
        private readonly StaticBuffer<ushort> _indexBuffer;

        private readonly ShaderResourceView _materialsBufferView;
        private readonly ShaderResourceView _texturesView;

        private readonly List<DrawList> _drawListsOpaque;
        private readonly List<DrawList> _drawListsTransparent;

        public string Name { get; }

        public ModelBone ParentBone { get; }

        public BoundingSphere BoundingSphere { get; }

        public ModelMeshMaterialPass[] MaterialPasses { get; }

        public bool Skinned { get; }

        internal ModelMesh(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            ModelBone parentBone)
        {
            Name = w3dMesh.Header.MeshName;
            ParentBone = parentBone;

            BoundingSphere = new BoundingSphere(
                w3dMesh.Header.SphCenter.ToVector3(),
                w3dMesh.Header.SphRadius);

            Skinned = w3dMesh.Header.Attributes.HasFlag(W3dMeshFlags.GeometryTypeSkin);

            _numVertices = (uint) w3dMesh.Vertices.Length;

            _vertexBuffer = CreateVertexBuffer(
                contentManager,
                uploadBatch,
                w3dMesh,
                Skinned);

            _numIndices = (uint) w3dMesh.Triangles.Length * 3;

            _indexBuffer = CreateIndexBuffer(
                contentManager,
                uploadBatch,
                w3dMesh);

            var materialsBuffer = CreateMaterialsBuffer(
                contentManager,
                uploadBatch,
                w3dMesh);

            var textures = CreateTextures(
                contentManager,
                uploadBatch,
                w3dMesh);

            _materialsBufferView = AddDisposable(ShaderResourceView.Create(contentManager.GraphicsDevice, materialsBuffer));
            _texturesView = AddDisposable(ShaderResourceView.Create(contentManager.GraphicsDevice, textures));

            MaterialPasses = new ModelMeshMaterialPass[w3dMesh.MaterialPasses.Length];
            for (var i = 0; i < MaterialPasses.Length; i++)
            {
                MaterialPasses[i] = AddDisposable(new ModelMeshMaterialPass(
                    contentManager,
                    uploadBatch,
                    w3dMesh,
                    w3dMesh.MaterialPasses[i]));
            }

            var uniquePipelineStates = MaterialPasses
                .SelectMany(x => x.MeshParts.Select(y => y.PipelineStateHandle))
                .Distinct()
                .ToList();

            List<DrawList> createDrawList(bool alphaBlended)
            {
                var result = new List<DrawList>();
                foreach (var pipelineState in uniquePipelineStates)
                {
                    if (pipelineState.EffectPipelineState.BlendState.Enabled != alphaBlended)
                    {
                        continue;
                    }

                    var materialPasses = MaterialPasses
                        .Where(x => x.MeshParts.Any(y => y.PipelineStateHandle == pipelineState))
                        .ToList();

                    if (materialPasses.Count > 0)
                    {
                        result.Add(new DrawList
                        {
                            PipelineState = pipelineState,
                            MaterialPasses = materialPasses
                        });
                    }
                }
                return result;
            }

            _drawListsOpaque = createDrawList(false);
            _drawListsTransparent = createDrawList(true);
        }

        private StaticBuffer<VertexMaterial> CreateMaterialsBuffer(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var vertexMaterials = new VertexMaterial[w3dMesh.Materials.Length];

            for (var i = 0; i < w3dMesh.Materials.Length; i++)
            {
                var w3dMaterial = w3dMesh.Materials[i];
                var w3dVertexMaterial = w3dMaterial.VertexMaterialInfo;

                vertexMaterials[i] = w3dVertexMaterial.ToVertexMaterial(w3dMaterial);
            }

            return AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                vertexMaterials));
        }

        private static Texture[] CreateTextures(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var numTextures = w3dMesh.Textures.Length;
            var textures = new Texture[numTextures];
            for (var i = 0; i < numTextures; i++)
            {
                var w3dTexture = w3dMesh.Textures[i];
                var w3dTextureFilePath = Path.Combine("Art", "Textures", w3dTexture.Name);
                textures[i] = contentManager.Load<Texture>(w3dTextureFilePath, uploadBatch);
            }
            return textures;
        }

        private StaticBuffer<MeshVertex> CreateVertexBuffer(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh,
            bool isSkinned)
        {
            var vertices = new MeshVertex[_numVertices];

            for (var i = 0; i < _numVertices; i++)
            {
                vertices[i] = new MeshVertex
                {
                    Position = w3dMesh.Vertices[i].ToVector3(),
                    Normal = w3dMesh.Normals[i].ToVector3(),
                    BoneIndex = isSkinned
                        ? w3dMesh.Influences[i].BoneIndex
                        : 0u
                };
            }

            return AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                vertices));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshVertex
        {
            public const int SizeInBytes = sizeof(float) * 6 + sizeof(uint);

            public Vector3 Position;
            public Vector3 Normal;
            public uint BoneIndex;
        }

        private StaticBuffer<ushort> CreateIndexBuffer(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dMesh w3dMesh)
        {
            var indices = new ushort[_numIndices];

            var indexIndex = 0;
            foreach (var triangle in w3dMesh.Triangles)
            {
                indices[indexIndex++] = (ushort) triangle.VIndex0;
                indices[indexIndex++] = (ushort) triangle.VIndex1;
                indices[indexIndex++] = (ushort) triangle.VIndex2;
            }

            return AddDisposable(StaticBuffer.Create(
                contentManager.GraphicsDevice,
                uploadBatch,
                indices));
        }

        public void Draw(
            CommandEncoder commandEncoder, 
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world,
            bool alphaBlended)
        {
            meshEffect.SetSkinningEnabled(Skinned);

            meshEffect.SetWorld(ref world);
            meshEffect.SetView(camera.ViewMatrix);
            meshEffect.SetProjection(camera.ProjectionMatrix);

            // TODO: Use time from main game engine, don't query for it every time like this.
            var timeInSeconds = (float) System.DateTime.Now.TimeOfDay.TotalSeconds;

            void drawImpl(EffectPipelineStateHandle pipelineStateHandle, IEnumerable<ModelMeshMaterialPass> materialPasses)
            {
                meshEffect.SetPipelineState(pipelineStateHandle);

                meshEffect.SetMaterials(_materialsBufferView);
                meshEffect.SetTextures(_texturesView);

                commandEncoder.SetVertexBuffer(0, _vertexBuffer);

                foreach (var materialPass in materialPasses)
                {
                    meshEffect.SetTextureIndices(materialPass.TextureIndicesBufferView);
                    meshEffect.SetMaterialIndices(materialPass.MaterialIndicesBufferView);

                    commandEncoder.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);

                    foreach (var meshPart in materialPass.MeshParts)
                    {
                        if (meshPart.PipelineStateHandle != pipelineStateHandle)
                        {
                            continue;
                        }

                        meshEffect.SetPrimitiveOffset(meshPart.StartIndex / 3);
                        meshEffect.SetNumTextureStages(materialPass.NumTextureStages);
                        meshEffect.SetAlphaTest(meshPart.AlphaTest);
                        meshEffect.SetTexturing(meshPart.Texturing);
                        meshEffect.SetTimeInSeconds(timeInSeconds);

                        meshEffect.Apply(commandEncoder);

                        commandEncoder.DrawIndexed(
                            PrimitiveType.TriangleList,
                            meshPart.IndexCount,
                            _indexBuffer,
                            meshPart.StartIndex);
                    }
                }
            }

            var drawLists = alphaBlended
                ? _drawListsTransparent
                : _drawListsOpaque;

            foreach (var drawList in drawLists)
            {
                drawImpl(drawList.PipelineState, drawList.MaterialPasses);
            }
        }

        private sealed class DrawList
        {
            public EffectPipelineStateHandle PipelineState;
            public List<ModelMeshMaterialPass> MaterialPasses;
        }
    }
}
