namespace OpenSage.Terrain
{
    public sealed class TerrainLogic
    {
        internal void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            reader.ReadVersion(2);

            reader.SkipUnknownBytes(8);
        }
    }
}
