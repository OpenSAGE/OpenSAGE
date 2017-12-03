using System.Collections.Generic;
using System.Numerics;
using LL.Graphics3D;
using OpenSage.Mathematics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.Rendering
{
    internal delegate void RenderCallback(
        CommandEncoder commandEncoder, 
        Effect effect, 
        EffectPipelineStateHandle pipelineStateHandle,
        RenderInstanceData instanceData);

    internal abstract class RenderItemBase
    {
        public readonly Effect Effect;
        public readonly EffectPipelineStateHandle PipelineStateHandle;
        public readonly RenderCallback RenderCallback;

        protected RenderItemBase(
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
        {
            Effect = effect;
            PipelineStateHandle = pipelineStateHandle;
            RenderCallback = renderCallback;
        }
    }

    internal sealed class RenderItem : RenderItemBase
    {
        public readonly EffectMaterial Material;
        public readonly RenderableComponent Renderable;

        // Set by RenderPipeline.
        public bool Visible;

        public RenderItem(
            RenderableComponent renderable,
            EffectMaterial material,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
            : base(material.Effect, pipelineStateHandle, renderCallback)
        {
            Material = material;
            Renderable = renderable;
        }
    }

    internal sealed class RenderInstanceData : DisposableBase
    {
        public readonly ModelMesh Mesh;
        public readonly List<InstancedRenderable> InstancedRenderables = new List<InstancedRenderable>();

        public Buffer<Matrix4x3> SkinningBuffer;
        public Buffer<Matrix4x4> WorldBuffer;

        private Matrix4x3[] _skinningBones;
        private Matrix4x4[] _worldTransforms;

        public uint NumInstances { get; private set; }

        public RenderInstanceData(ModelMesh mesh)
        {
            Mesh = mesh;
        }

        public void Update(GraphicsDevice graphicsDevice, CameraComponent camera)
        {
            NumInstances = 0;

            foreach (var instancedRenderable in InstancedRenderables)
            {
                if (instancedRenderable.Visible)
                {
                    NumInstances += 1;
                }
            }

            if (NumInstances == 0)
            {
                return;
            }

            if (Mesh.Skinned)
            {
                var numElements = (int) (Mesh.NumBones * NumInstances);
                if (SkinningBuffer == null || SkinningBuffer.ElementCount < numElements)
                {
                    RemoveAndDispose(SkinningBuffer);

                    SkinningBuffer = AddDisposable(Buffer<Matrix4x3>.CreateDynamicArray(
                        graphicsDevice, 
                        numElements, 
                        BufferBindFlags.ShaderResource));

                    _skinningBones = new Matrix4x3[numElements];
                }

                var boneIndex = 0;
                foreach (var instancedRenderable in InstancedRenderables)
                {
                    if (instancedRenderable.Visible)
                    {
                        for (var i = 0; i < Mesh.NumBones; i++)
                        {
                            // Bone matrix should be relative to root bone transform.
                            var rootBoneMatrix = instancedRenderable.Bones[0].LocalToWorldMatrix;
                            var boneMatrix = instancedRenderable.Bones[i].LocalToWorldMatrix;

                            var boneMatrixRelativeToRoot = boneMatrix * Matrix4x4Utility.Invert(rootBoneMatrix);

                            boneMatrixRelativeToRoot.ToMatrix4x3(out _skinningBones[boneIndex++]);
                        }
                    }
                }

                SkinningBuffer.SetData(_skinningBones);
            }

            if (WorldBuffer == null || WorldBuffer.ElementCount < NumInstances)
            {
                RemoveAndDispose(WorldBuffer);

                WorldBuffer = AddDisposable(Buffer<Matrix4x4>.CreateDynamicArray(
                    graphicsDevice,
                    (int) NumInstances, 
                    BufferBindFlags.VertexBuffer));

                _worldTransforms = new Matrix4x4[NumInstances];
            }

            var worldTransformIndex = 0;
            foreach (var instancedRenderable in InstancedRenderables)
            {
                if (instancedRenderable.Visible)
                {
                    if (instancedRenderable.Renderable is MeshComponent m && m.Mesh.CameraOriented)
                    {
                        var localToWorldMatrix = instancedRenderable.Renderable.Transform.LocalToWorldMatrix;

                        var viewInverse = Matrix4x4Utility.Invert(camera.View);
                        var cameraPosition = viewInverse.Translation;

                        var toCamera = Vector3.Normalize(Vector3.TransformNormal(
                            cameraPosition - instancedRenderable.Renderable.Transform.WorldPosition,
                            instancedRenderable.Renderable.Transform.WorldToLocalMatrix));

                        toCamera.Z = 0;

                        var cameraOrientedRotation = Matrix4x4.CreateFromQuaternion(QuaternionUtility.CreateRotation(Vector3.UnitX, toCamera));

                        var world = cameraOrientedRotation * localToWorldMatrix;

                        _worldTransforms[worldTransformIndex++] = world;
                    }
                    else
                    {
                        _worldTransforms[worldTransformIndex++] = instancedRenderable.Renderable.Transform.LocalToWorldMatrix;
                    }
                }
            }

            WorldBuffer.SetData(_worldTransforms);
        }
    }

    internal sealed class InstancedRenderItem : RenderItemBase
    {
        public readonly RenderInstanceData InstanceData;

        public InstancedRenderItem(
            RenderInstanceData instanceData,
            Effect effect,
            EffectPipelineStateHandle pipelineStateHandle,
            RenderCallback renderCallback)
            : base(effect, pipelineStateHandle, renderCallback)
        {
            InstanceData = instanceData;
        }
    }

    internal sealed class InstancedRenderable
    {
        public readonly RenderableComponent Renderable;
        public readonly TransformComponent[] Bones;

        // Set by RenderPipeline.
        public bool Visible;

        public InstancedRenderable(RenderableComponent renderable)
        {
            Renderable = renderable;
            if (renderable.MeshBase.Skinned)
            {
                Bones = renderable.Entity.GetComponent<ModelComponent>().Bones;
            }
        }
    }
}
