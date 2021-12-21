namespace OpenSage.Terrain
{
    public sealed class TerrainLogic
    {
        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadVersion(2);

            reader.SkipUnknownBytes(8);
        }
    }
}
