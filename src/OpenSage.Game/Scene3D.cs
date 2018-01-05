namespace OpenSage
{
    public sealed class Scene3D
    {
        public World World { get; }

        internal Scene3D(World world)
        {
            World = world;
        }
    }
}
