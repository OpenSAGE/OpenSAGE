namespace OpenSage.Terrain
{
    public sealed class TerrainLogic : IPersistableObject
    {
        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            reader.PersistVersion(2);
            reader.EndObject();

            reader.SkipUnknownBytes(8);
        }
    }
}
