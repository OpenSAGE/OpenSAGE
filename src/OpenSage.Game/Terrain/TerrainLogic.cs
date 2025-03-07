using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain;

public sealed class TerrainLogic : IPersistableObject
{
    public HeightMap HeightMap { get; private set; }

    public void SetHeightMapData(HeightMapData heightMapData)
    {
        HeightMap = new HeightMap(heightMapData);
    }

    // TODO(Port): Implement this.
    public bool IsCliffCell(Vector2 worldPosition) => false;

    // TODO(Port): Implement this.
    public bool IsUnderwater(Vector2 worldPosition) => false;

    // TODO(Port): Implement this.
    public float GetLayerHeight(float x, float y, PathfindLayerType layer)
    {
        return HeightMap.GetHeight(x, y);
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
