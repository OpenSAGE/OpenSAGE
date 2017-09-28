using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        internal const int MaxBones = 100;

        public BoundingSphere BoundingSphere { get; }

        public ModelBone[] Bones { get; }

        public ModelMesh[] Meshes { get; }

        public Animation[] Animations { get; }

        public bool HasHierarchy { get; }

        internal Model(
            bool hasHierarchy,
            ModelBone[] bones,
            ModelMesh[] meshes,
            Animation[] animations,
            BoundingSphere boundingSphere)
        {
            HasHierarchy = hasHierarchy;

            Bones = bones;

            foreach (var mesh in meshes)
            {
                AddDisposable(mesh);
            }
            Meshes = meshes;

            Animations = animations;

            BoundingSphere = boundingSphere;
        }
    }
}
