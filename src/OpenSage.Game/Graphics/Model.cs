using OpenSage.Data.W3d;
using LLGfx;
using System.Collections.Generic;
using OpenSage.Data;
using System.Numerics;
using OpenSage.Graphics.Util;
using System.Linq;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        public Vector3 BoundingSphereCenter { get; }
        public float BoundingSphereRadius { get; }

        public ModelBoneCollection Bones { get; }

        public ModelBone Root { get; }

        public IReadOnlyList<ModelMesh> Meshes { get; }

        internal Model(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            W3dFile w3dFile,
            FileSystem fileSystem,
            DescriptorSetLayout pixelMeshDescriptorSetLayout,
            DescriptorSetLayout vertexMaterialPassDescriptorSetLayout,
            DescriptorSetLayout pixelMaterialPassDescriptorSetLayout)
        {
            var bones = new List<ModelBone>();

            if (w3dFile.Hierarchies.Length > 0)
            {
                var w3dHierarchy = w3dFile.Hierarchies[0];

                BoundingSphereCenter = w3dHierarchy.Header.Center.ToVector3();

                var children = new List<ModelBone>[w3dHierarchy.Pivots.Length];

                for (var i = 0; i < w3dHierarchy.Pivots.Length; i++)
                {
                    children[i] = new List<ModelBone>();

                    var pivot = w3dHierarchy.Pivots[i];

                    var parent = pivot.ParentIdx == -1
                        ? null
                        : bones[pivot.ParentIdx];

                    var translation = pivot.Translation.ToVector3();
                    var translationY = translation.Y;
                    translation.Y = translation.Z;
                    translation.Z = -translationY;

                    // TODO: Use quaternion, not euler angles
                    var eulerAngles = pivot.EulerAngles.ToVector3();
                    var eulerAnglesY = eulerAngles.Y;
                    eulerAngles.Y = eulerAngles.Z;
                    eulerAngles.Z = -eulerAnglesY;

                    var transform = Matrix4x4.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z) * Matrix4x4.CreateTranslation(translation);

                    var bone = new ModelBone(i, pivot.Name, parent, transform);

                    if (parent != null)
                    {
                        children[pivot.ParentIdx].Add(bone);
                    }

                    bones.Add(bone);
                }

                for (var i = 0; i < w3dHierarchy.Pivots.Length; i++)
                {
                    bones[i].Children = new ModelBoneCollection(children[i]);
                }
            }

            Bones = new ModelBoneCollection(bones);

            var meshes = new List<ModelMesh>();

            foreach (var w3dMesh in w3dFile.Meshes)
            {
                var bone = Bones.SingleOrDefault(x => x.Name == w3dMesh.Header.MeshName) ?? Bones[0];

                var mesh = new ModelMesh(
                    graphicsDevice, 
                    uploadBatch, 
                    w3dMesh,
                    bone,
                    fileSystem,
                    pixelMeshDescriptorSetLayout,
                    vertexMaterialPassDescriptorSetLayout,
                    pixelMaterialPassDescriptorSetLayout);
                meshes.Add(mesh);

                var radius = Vector3.Distance(BoundingSphereCenter, mesh.BoundingSphereCenter) + mesh.BoundingSphereRadius;
                if (radius > BoundingSphereRadius)
                {
                    BoundingSphereRadius = radius;
                }
            }

            Meshes = meshes;
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

        public void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            // TODO: Don't allocate this on every draw.
            var boneMatrices = new Matrix4x4[Bones.Count];

            CopyAbsoluteBoneTransformsTo(boneMatrices);

            foreach (var mesh in Meshes)
            {
                var meshWorld = boneMatrices[mesh.ParentBone.Index] * world;

                mesh.SetMatrices(ref meshWorld, ref view, ref projection);

                mesh.Draw(commandEncoder);
            }
        }
    }
}
