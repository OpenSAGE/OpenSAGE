using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    /// <summary>
    /// A mesh is composed of the following hierarchy:
    /// 
    /// - Mesh: Vertices, Normals, Indices, Materials.
    ///   - MeshMaterialPasses[]: per-vertex TexCoords, 
    ///                           per-vertex Material indices, 
    ///                           per-triangle Texture indices.
    ///     - MeshParts[]: One for each unique PipelineState in a material pass.
    ///                    StartIndex, IndexCount, PipelineState, AlphaTest, Texturing
    /// </summary>
    public sealed class ModelMesh : DisposableBase
    {
        private readonly ShaderSet _shaderSet;
        private readonly ShaderSet _depthShaderSet;

        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;

        private readonly ConstantBuffer<MeshTypes.MeshConstants> _meshConstantsBuffer;
        private readonly ResourceSet _meshConstantsResourceSet;

        private readonly ResourceSet _samplerResourceSet;

        public readonly string Name;

        public readonly BoundingBox BoundingBox;

        internal readonly List<ModelMeshMaterialPass> MaterialPasses;

        public readonly bool Skinned;

        public readonly bool Hidden;
        public readonly bool CameraOriented;

        internal ModelMesh(
            ContentManager contentManager,
            string name,
            ShaderSet shaderSet,
            ReadOnlySpan<byte> vertexData,
            ushort[] indices,
            List<ModelMeshMaterialPass> materialPasses,
            bool isSkinned,
            BoundingBox boundingBox,
            bool hidden,
            bool cameraOriented,
            bool hasHouseColor)
        {
            Name = name;

            _shaderSet = shaderSet;
            _depthShaderSet = contentManager.ShaderLibrary.MeshDepth;

            BoundingBox = boundingBox;

            Skinned = isSkinned;

            Hidden = hidden;
            CameraOriented = cameraOriented;

            var graphicsDevice = contentManager.GraphicsDevice;

            _vertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(vertexData, BufferUsage.VertexBuffer));

            _indexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();

            commandList.Begin();

            _meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshTypes.MeshConstants>(graphicsDevice));
            _meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
            _meshConstantsBuffer.Value.HasHouseColor = hasHouseColor;
            _meshConstantsBuffer.Update(commandList);

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            graphicsDevice.DisposeWhenIdle(commandList);

            _meshConstantsResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    contentManager.ShaderLibrary.FixedFunction.ResourceLayouts[4],
                    _meshConstantsBuffer.Buffer)));

            _samplerResourceSet = contentManager.FixedFunctionResourceCache.SamplerResourceSet;

            foreach (var materialPass in materialPasses)
            {
                AddDisposable(materialPass);
            }
            MaterialPasses = materialPasses;
        }

        internal void BuildRenderList(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelBone parentBone,
            in Matrix4x4 modelTransform,
            bool castsShadow,
            Player owner)
        {
            var meshWorldMatrix = Skinned
                ? modelTransform
                : modelInstance.AbsoluteBoneTransforms[parentBone.Index];

            BuildRenderListWithWorldMatrix(
                renderList,
                camera,
                modelInstance,
                parentBone,
                meshWorldMatrix,
                castsShadow,
                owner);
        }

        internal void BuildRenderListWithWorldMatrix(
            RenderList renderList,
            Camera camera,
            ModelInstance modelInstance,
            ModelBone parentBone,
            in Matrix4x4 meshWorldMatrix,
            bool castsShadow,
            Player owner = null)
        {
            if (Hidden)
            {
                return;
            }

            if (!modelInstance.BoneVisibilities[parentBone.Index])
            {
                return;
            }

            Matrix4x4 world;
            if (CameraOriented)
            {
                // TODO: I don't think this is correct yet.

                var localToWorldMatrix = meshWorldMatrix;

                var viewInverse = Matrix4x4Utility.Invert(camera.View);
                var cameraPosition = viewInverse.Translation;

                var toCamera = Vector3.Normalize(Vector3.TransformNormal(
                    cameraPosition - meshWorldMatrix.Translation,
                    Matrix4x4Utility.Invert(meshWorldMatrix)));

                toCamera.Z = 0;

                var cameraOrientedRotation = Matrix4x4.CreateFromQuaternion(
                    QuaternionUtility.CreateRotation(
                        Vector3.UnitX,
                        toCamera));

                world = cameraOrientedRotation * localToWorldMatrix;
            }
            else
            {
                world = meshWorldMatrix;
            }

            var meshBoundingBox = BoundingBox.Transform(BoundingBox, world); // TODO: Not right for skinned meshes

            foreach (var materialPass in MaterialPasses)
            {
                foreach (var meshPart in materialPass.MeshParts)
                {
                    var blendEnabled = meshPart.BlendEnabled;

                    // Depth pass

                    // TODO: With more work, we could draw shadows for translucent and alpha-tested materials.
                    if (!blendEnabled && castsShadow)
                    {
                        //meshPart.DepthMaterial.SetSkinningBuffer(modelInstance.SkinningBuffer);

                        renderList.Shadow.RenderItems.Add(new RenderItem(
                           _depthShaderSet,
                           meshPart.DepthPipeline,
                           meshBoundingBox,
                           world,
                           meshPart.StartIndex,
                           meshPart.IndexCount,
                           _indexBuffer,
                           cl =>
                           {
                               cl.SetGraphicsResourceSet(1, _meshConstantsResourceSet);
                               cl.SetGraphicsResourceSet(3, modelInstance.SkinningBufferResourceSet);

                               cl.SetVertexBuffer(0, _vertexBuffer);

                               if (materialPass.TexCoordVertexBuffer != null)
                               {
                                   cl.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);
                               }
                           }));
                    }

                    // Standard pass

                    var renderQueue = blendEnabled
                        ? renderList.Transparent
                        : renderList.Opaque;

                    renderQueue.RenderItems.Add(new RenderItem(
                        _shaderSet,
                        meshPart.Pipeline,
                        meshBoundingBox,
                        world,
                        meshPart.StartIndex,
                        meshPart.IndexCount,
                        _indexBuffer,
                        cl =>
                        {
                            cl.SetGraphicsResourceSet(4, _meshConstantsResourceSet);
                            cl.SetGraphicsResourceSet(5, meshPart.MaterialResourceSet);
                            cl.SetGraphicsResourceSet(6, _samplerResourceSet);
                            cl.SetGraphicsResourceSet(8, modelInstance.SkinningBufferResourceSet);

                            cl.SetVertexBuffer(0, _vertexBuffer);

                            if (materialPass.TexCoordVertexBuffer != null)
                            {
                                cl.SetVertexBuffer(1, materialPass.TexCoordVertexBuffer);
                            }
                        },
                        owner?.Color));
                }
            }
        }
    }
}
