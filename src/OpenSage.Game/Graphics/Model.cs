using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class Model : DisposableBase
    {
        public ModelBone[] Bones { get; }
        public ModelMesh[] Meshes { get; }
        public Animation.Animation[] Animations { get; }

        internal Model(
            ModelBone[] bones,
            ModelMesh[] meshes,
            Animation.Animation[] animations)
        {
            Bones = bones;

            foreach (var mesh in meshes)
            {
                AddDisposable(mesh);
            }
            Meshes = meshes;

            Animations = animations;
        }

        public ModelInstance CreateInstance(GraphicsDevice graphicsDevice)
        {
            return new ModelInstance(this, graphicsDevice);
        }
    }
}
