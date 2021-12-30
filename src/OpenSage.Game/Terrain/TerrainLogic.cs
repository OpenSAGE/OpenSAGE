namespace OpenSage.Terrain
{
    public sealed class TerrainLogic
    {
        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVersion(2);

            reader.SkipUnknownBytes(8);
        }
    }
}
