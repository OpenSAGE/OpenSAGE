using OpenSage.Data.W3d;
using LLGfx;
using System.Collections.Generic;
using OpenSage.Data;
using System.Numerics;
using OpenSage.Graphics.Util;
using System.Linq;
using System;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        private Matrix4x4[] _absoluteBoneMatrices;

        private readonly DynamicBuffer _skinningConstantBuffer;
        private SkinningConstants _skinningConstants;

        public Vector3 BoundingSphereCenter { get; }
        public float BoundingSphereRadius { get; }

        public ModelBoneCollection Bones { get; }

        public Matrix4x4[] AnimatedBoneTransforms { get; }

        public ModelBone Root { get; }

        public Transform[] BindPose { get; }

        public Matrix4x4[] InverseBindPose { get; }

        public IReadOnlyList<ModelMesh> Meshes { get; }

        public IReadOnlyList<Animation> Animations { get; }

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
            var bones = new List<ModelBone>();

            var w3dHierarchy = w3dFile.Hierarchy;
            if (w3dFile.HLod != null && w3dHierarchy == null)
            {
                // Load referenced hierarchy.
                var hierarchyFileName = w3dFile.HLod.Header.HierarchyName + ".W3D";
                var hierarchyFileEntry = fileSystem.GetFile($@"Art\W3D\{hierarchyFileName}");
                using (var hierarchyFileStream = hierarchyFileEntry.Open())
                {
                    var hierarchyFile = W3dFile.FromStream(hierarchyFileStream);
                    w3dHierarchy = hierarchyFile.Hierarchy;
                }
            }

            if (w3dHierarchy != null)
            {
                BoundingSphereCenter = w3dHierarchy.Header.Center.ToVector3();

                if (w3dHierarchy.Pivots.Length > MaxBones)
                {
                    throw new NotSupportedException();
                }

                var children = new List<ModelBone>[w3dHierarchy.Pivots.Length];

                var bindPose = new List<Transform>();
                var inverseBindPose = new List<Matrix4x4>();

                for (var i = 0; i < w3dHierarchy.Pivots.Length; i++)
                {
                    children[i] = new List<ModelBone>();

                    var pivot = w3dHierarchy.Pivots[i];

                    var parent = pivot.ParentIdx == -1
                        ? null
                        : bones[pivot.ParentIdx];

                    var translation = pivot.Translation.ToVector3();
                    var rotation = pivot.Rotation.ToQuaternion();

                    bindPose.Add(new Transform
                    {
                        Translation = translation,
                        Rotation = rotation
                    });

                    var bone = new ModelBone(i, pivot.Name, parent, translation, rotation);

                    var absoluteTransform = bone.Transform;
                    var p = parent;
                    while (p != null)
                    {
                        absoluteTransform = absoluteTransform * p.Transform;
                        p = p.Parent;
                    }
                    Matrix4x4.Invert(absoluteTransform, out var inverseAbsoluteTransform);
                    inverseBindPose.Add(inverseAbsoluteTransform);

                    if (parent != null)
                    {
                        children[pivot.ParentIdx].Add(bone);
                    }

                    bones.Add(bone);
                }

                BindPose = bindPose.ToArray();
                InverseBindPose = inverseBindPose.ToArray();

                for (var i = 0; i < w3dHierarchy.Pivots.Length; i++)
                {
                    bones[i].Children = new ModelBoneCollection(children[i]);
                }
            }
            else
            {
                BindPose = new Transform[0];
                InverseBindPose = new Matrix4x4[0];
            }

            Bones = new ModelBoneCollection(bones);

            _absoluteBoneMatrices = new Matrix4x4[bones.Count];
            CopyAbsoluteBoneTransformsTo(_absoluteBoneMatrices);

            AnimatedBoneTransforms = new Matrix4x4[bones.Count];
            for (var i = 0; i < bones.Count; i++)
            {
                AnimatedBoneTransforms[i] = Matrix4x4.Identity;
            }

            var meshes = new List<ModelMesh>();

            foreach (var w3dMesh in w3dFile.Meshes)
            {
                var hlodSubObjct = w3dFile.HLod.Lods[0].SubObjects.Single(x => x.Name == w3dMesh.Header.ContainerName + "." + w3dMesh.Header.MeshName);
                var bone = Bones[(int) hlodSubObjct.BoneIndex];

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
                meshes.Add(mesh);

                var radius = Vector3.Distance(BoundingSphereCenter, mesh.BoundingSphereCenter) + mesh.BoundingSphereRadius;
                if (radius > BoundingSphereRadius)
                {
                    BoundingSphereRadius = radius;
                }
            }

            Meshes = meshes;

            var w3dAnimations = w3dFile.Animations.ToList();

            if (w3dFile.HLod.Header.Name.EndsWith("_SKN"))
            {
                var namePrefix = w3dFile.HLod.Header.Name.Substring(0, w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
                foreach (var animationFileEntry in fileSystem.GetFiles(@"Art\W3D"))
                {
                    if (!animationFileEntry.FilePath.StartsWith($@"Art\W3D\{namePrefix}"))
                    {
                        continue;
                    }

                    using (var animationFileStream = animationFileEntry.Open())
                    {
                        var animationFile = W3dFile.FromStream(animationFileStream);
                        w3dAnimations.AddRange(animationFile.Animations);
                    }
                }
            }

            var animations = new List<Animation>();

            foreach (var w3dAnimation in w3dAnimations)
            {
                var clips = new List<AnimationClip>();

                foreach (var w3dChannel in w3dAnimation.Channels)
                {
                    var keyframes = new List<Keyframe>();

                    var data = w3dChannel.Data;
                    for (var i = 0; i < data.GetLength(0); i++)
                    {
                        var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);

                        // Switch y and z to account for z being up in .w3d
                        switch (w3dChannel.Flags)
                        {
                            case W3dAnimationChannelType.Quaternion:
                                keyframes.Add(new QuaternionKeyframe(
                                    time,
                                    new Quaternion(data[i, 0], data[i, 2], -data[i, 1], data[i, 3])));
                                break;

                            case W3dAnimationChannelType.TranslationX:
                                keyframes.Add(new TranslationXKeyframe(time, data[i, 0]));
                                break;

                            case W3dAnimationChannelType.TranslationY:
                                keyframes.Add(new TranslationZKeyframe(time, -data[i, 0]));
                                break;

                            case W3dAnimationChannelType.TranslationZ:
                                keyframes.Add(new TranslationYKeyframe(time, data[i, 0]));
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }

                    clips.Add(new AnimationClip(
                        w3dChannel.Pivot,
                        keyframes.ToArray()));
                }

                animations.Add(new Animation(
                    w3dAnimation.Header.Name,
                    TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate),
                    clips.ToArray()));
            }

            Animations = animations;

            _skinningConstantBuffer = AddDisposable(DynamicBuffer.Create<SkinningConstants>(graphicsDevice));
        }

        public void CopyAbsoluteBoneTransformsTo(Matrix4x4[] destinationBoneTransforms)
        {
            for (var i = 0; i < Bones.Count; i++)
            {
                var bone = Bones[i];

                destinationBoneTransforms[i] = bone.Parent != null
                    ? bone.Transform * destinationBoneTransforms[bone.Parent.Index]
                    : bone.Transform;
            }
        }

        public void PreDraw(CommandEncoder commandEncoder)
        {
            for (var i = 0; i < Bones.Count; i++)
            {
                var bone = Bones[i];

                _absoluteBoneMatrices[i] = bone.Parent != null
                    ? AnimatedBoneTransforms[i] * bone.Transform * _absoluteBoneMatrices[bone.Parent.Index]
                    : AnimatedBoneTransforms[i] * bone.Transform;
            }

            if (Meshes.Any(x => x.Skinned))
            {
                unsafe
                {
                    fixed (float* boneArray = _skinningConstants.Bones)
                    {
                        for (var i = 0; i < _absoluteBoneMatrices.Length; i++)
                        {
                            var m = _absoluteBoneMatrices[i];

                            var baseOffset = boneArray + (i * 16);

                            *(baseOffset + 0) = m.M11;
                            *(baseOffset + 1) = m.M12;
                            *(baseOffset + 2) = m.M13;
                            *(baseOffset + 3) = m.M14;

                            *(baseOffset + 4) = m.M21;
                            *(baseOffset + 5) = m.M22;
                            *(baseOffset + 6) = m.M23;
                            *(baseOffset + 7) = m.M24;

                            *(baseOffset + 8) = m.M31;
                            *(baseOffset + 9) = m.M32;
                            *(baseOffset + 10) = m.M33;
                            *(baseOffset + 11) = m.M34;

                            *(baseOffset + 12) = m.M41;
                            *(baseOffset + 13) = m.M42;
                            *(baseOffset + 14) = m.M43;
                            *(baseOffset + 15) = m.M44;
                        }
                    }
                }

                _skinningConstantBuffer.SetData(ref _skinningConstants);

                commandEncoder.SetInlineConstantBuffer(2, _skinningConstantBuffer);
            }
        }

        private const int MaxBones = 72;

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct SkinningConstants
        {
            // Array of MaxBones * float4x4
            public fixed float Bones[MaxBones * 16];
        }

        public void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            var localWorld = world;
            var localView = view;
            var localProjection = projection;

            void drawImpl(bool alphaBlended)
            {
                foreach (var mesh in Meshes)
                {
                    var meshWorld = mesh.Skinned
                        ? localWorld
                        : _absoluteBoneMatrices[mesh.ParentBone.Index] * localWorld;

                    mesh.SetMatrices(ref meshWorld, ref localView, ref localProjection);

                    mesh.Draw(commandEncoder, alphaBlended);
                }
            }

            drawImpl(false);
            drawImpl(true);
        }
    }
}
