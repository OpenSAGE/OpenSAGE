using OpenSage.Data.Map;

namespace OpenSage.Terrain
{
    public sealed class TerrainLogic : IPersistableObject
    {
        public HeightMap HeightMap { get; private set; }

        public void SetHeightMapData(HeightMapData heightMapData)
        {
            HeightMap = new HeightMap(heightMapData);
        }

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
