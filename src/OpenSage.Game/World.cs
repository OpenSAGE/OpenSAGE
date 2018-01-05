namespace OpenSage
{
    public sealed class World
    {
        public Terrain.Terrain Terrain { get; }

        internal World(Terrain.Terrain terrain)
        {
            Terrain = terrain;
        }
    }
}
