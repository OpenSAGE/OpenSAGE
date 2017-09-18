using System;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        public const int MaxBones = 100;

        public BoundingSphere BoundingSphere { get; }

        public ModelBone[] Bones { get; }

        public ModelBone Root { get; }

        public ModelMesh[] Meshes { get; }

        public Animation[] Animations { get; }

        public bool HasHierarchy { get; }

        internal Model(
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            W3dFile w3dFile,
            W3dHierarchyDef w3dHierarchy)
        {
            HasHierarchy = w3dHierarchy != null;

            if (w3dHierarchy != null)
            {
                if (w3dHierarchy.Pivots.Length > MaxBones)
                {
                    throw new NotSupportedException();
                }

                Bones = new ModelBone[w3dHierarchy.Pivots.Length];

                for (var i = 0; i < Bones.Length; i++)
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
                Bones = new ModelBone[1];
                Bones[0] = new ModelBone(0, null, null, Vector3.Zero, Quaternion.Identity);
            }

            Meshes = new ModelMesh[w3dFile.Meshes.Count];
            for (var i = 0; i < Meshes.Length; i++)
            {
                var w3dMesh = w3dFile.Meshes[i];

                ModelBone bone;
                if (w3dFile.HLod != null)
                {
                    var hlodSubObject = w3dFile.HLod.Lods[0].SubObjects.Single(x => x.Name == w3dMesh.Header.ContainerName + "." + w3dMesh.Header.MeshName);
                    bone = Bones[(int) hlodSubObject.BoneIndex];
                }
                else
                {
                    bone = Bones[0];
                }

                var mesh = AddDisposable(new ModelMesh(
                    contentManager,
                    uploadBatch,
                    w3dMesh,
                    bone));
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
    }
}
