using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        private Matrix4x4[] _absoluteBoneMatrices;

        private readonly DynamicBuffer _skinningConstantBuffer;
        private SkinningConstants _skinningConstants;

        public BoundingSphere BoundingSphere { get; }

        public ModelBone[] Bones { get; }

        public Matrix4x4[] AnimatedBoneTransforms { get; }
        public bool[] AnimatedBoneVisibilities { get; }

        public ModelBone Root { get; }

        public ModelMesh[] Meshes { get; }

        public Animation[] Animations { get; }

        internal Model(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dFile w3dFile,
            FileSystem fileSystem,
            DescriptorSetLayout pixelMeshDescriptorSetLayout,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout,
            ModelRenderer modelRenderer)
        {
            var w3dHierarchy = w3dFile.Hierarchy;
            if (w3dFile.HLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                var hierarchyFileName = w3dFile.HLod.Header.HierarchyName + ".W3D";
                var hierarchyFilePath = Path.Combine(Path.GetDirectoryName(w3dFile.FilePath), hierarchyFileName);
                var hierarchyFileEntry = fileSystem.GetFile(hierarchyFilePath);
                var hierarchyFile = W3dFile.FromFileSystemEntry(hierarchyFileEntry);
                w3dHierarchy = hierarchyFile.Hierarchy;
            }

            if (w3dHierarchy != null)
            {
                if (w3dHierarchy.Pivots.Length > MaxBones)
                {
                    throw new NotSupportedException();
                }

                Bones = new ModelBone[w3dHierarchy.Pivots.Length];

                for (var i = 0; i < w3dHierarchy.Pivots.Length; i++)
                {
                    var pivot = w3dHierarchy.Pivots[i];

                    var parent = pivot.ParentIdx == -1
                        ? null
                        : Bones[pivot.ParentIdx];

                    Bones[i] = new ModelBone(
                        i, 
                        pivot.Name, 
                        parent,
                        pivot.Translation.ToVector3(),
                        pivot.Rotation.ToQuaternion());
                }
            }
            else
            {
                Bones = new ModelBone[0];
            }

            _absoluteBoneMatrices = new Matrix4x4[Bones.Length];

            AnimatedBoneTransforms = new Matrix4x4[Bones.Length];
            for (var i = 0; i < Bones.Length; i++)
            {
                AnimatedBoneTransforms[i] = Matrix4x4.Identity;
            }

            AnimatedBoneVisibilities = new bool[Bones.Length];
            for (var i = 0; i < Bones.Length; i++)
            {
                AnimatedBoneVisibilities[i] = true;
            }

            _skinningConstantBuffer = AddDisposable(DynamicBuffer.Create<SkinningConstants>(graphicsDevice));

            Meshes = new ModelMesh[w3dFile.Meshes.Count];
            for (var i = 0; i < w3dFile.Meshes.Count; i++)
            {
                var w3dMesh = w3dFile.Meshes[i];

                var hlodSubObject = w3dFile.HLod.Lods[0].SubObjects.Single(x => x.Name == w3dMesh.Header.ContainerName + "." + w3dMesh.Header.MeshName);
                var bone = Bones[(int) hlodSubObject.BoneIndex];

                var mesh = new ModelMesh(
                    graphicsDevice, 
                    uploadBatch, 
                    w3dMesh,
                    bone,
                    fileSystem,
                    pixelMeshDescriptorSetLayout,
                    vertexMaterialPassDescriptorSetLayout,
                    pixelMaterialPassDescriptorSetLayout,
                    modelRenderer);
                Meshes[i] = mesh;

                var meshBoundingSphere = mesh.BoundingSphere.Transform(bone.Transform);

                BoundingSphere = (i == 0)
                    ? meshBoundingSphere
                    : BoundingSphere.CreateMerged(BoundingSphere, meshBoundingSphere);
            }

            Animations = new Animation[w3dFile.Animations.Count + w3dFile.CompressedAnimations.Count];
            for (var i = 0; i < w3dFile.Animations.Count; i++)
            {
                Animations[i] = new Animation(w3dFile.Animations[i]);
            }
            for (var i = 0; i < w3dFile.CompressedAnimations.Count; i++)
            {
                Animations[w3dFile.Animations.Count + i] = new Animation(w3dFile.CompressedAnimations[i]);
            }
        }

        public void PreDraw(CommandEncoder commandEncoder)
        {
            for (var i = 0; i < Bones.Length; i++)
            {
                var bone = Bones[i];

                _absoluteBoneMatrices[i] = bone.Parent != null
                    ? AnimatedBoneTransforms[i] * bone.Transform * _absoluteBoneMatrices[bone.Parent.Index]
                    : AnimatedBoneTransforms[i] * bone.Transform;
            }

            if (Meshes.Any(x => x.Skinned))
            {
                _skinningConstants.CopyFrom(_absoluteBoneMatrices);
                _skinningConstantBuffer.SetData(ref _skinningConstants);
                commandEncoder.SetInlineConstantBuffer(2, _skinningConstantBuffer);
            }
        }

        private const int MaxBones = 72;

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct SkinningConstants
        {
            // Array of MaxBones * float4x3
            public fixed float Bones[MaxBones * 12];

            public void CopyFrom(Matrix4x4[] matrices)
            {
                fixed (float* boneArray = Bones)
                {
                    for (var i = 0; i < matrices.Length; i++)
                    {
                        PointerUtil.CopyToMatrix4x3(
                            ref matrices[i],
                            boneArray + (i * 12));
                    }
                }
            }
        }

        public void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            DrawImpl(commandEncoder, ref world, ref view, ref projection, false);
            DrawImpl(commandEncoder, ref world, ref view, ref projection, true);
        }

        private void DrawImpl(
            CommandEncoder commandEncoder,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection,
            bool alphaBlended)
        {
            foreach (var mesh in Meshes)
            {
                if (!AnimatedBoneVisibilities[mesh.ParentBone.Index])
                {
                    continue;
                }

                var meshWorld = mesh.Skinned
                    ? world
                    : _absoluteBoneMatrices[mesh.ParentBone.Index] * world;

                mesh.SetMatrices(ref meshWorld, ref view, ref projection);

                mesh.Draw(commandEncoder, alphaBlended);
            }
        }
    }
}
