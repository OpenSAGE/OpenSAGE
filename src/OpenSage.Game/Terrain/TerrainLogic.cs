using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Terrain;

public sealed class TerrainLogic : IPersistableObject
{
    public HeightMap HeightMap { get; private set; }

    private int _activeBoundary = 0;

    public void SetHeightMapData(HeightMapData heightMapData)
    {
        HeightMap = new HeightMap(heightMapData);
    }

    // TODO(Port): Implement this.
    public bool IsCliffCell(Vector2 worldPosition) => false;

    // TODO(Port): Implement this.
    public bool IsUnderwater(Vector2 worldPosition, out float waterZ)
    {
        waterZ = 0;
        return false;
    }

    // TODO(Port): Implement this.
    public float GetLayerHeight(float x, float y, PathfindLayerType layer)
    {
        return HeightMap.GetHeight(x, y);
    }

    public float GetGroundHeight(float x, float y, out Vector3 normal)
    {
        normal = HeightMap.GetNormal(x, y);
        return HeightMap.GetHeight(x, y);
    }

    public float GetGroundHeight(float x, float y)
    {
        return HeightMap.GetHeight(x, y);
    }

    //TODO(Port): Implement actual logic.
    public void SetActiveBoundary(int newActiveBoundary)
    {
        _activeBoundary = newActiveBoundary;
    }

    public AxisAlignedBoundingBox GetExtent()
    {
        var maxXY = HeightMap.Boundaries.Length switch
        {
            0 => Vector2.Zero,
            _ => HeightMap.Boundaries[_activeBoundary].ToVector2() * HeightMap.HorizontalScale
        };

        var min = new Vector3(0, 0, HeightMap.MinZ);
        var max = new Vector3(maxXY.X, maxXY.Y, HeightMap.MaxZ);
        return new AxisAlignedBoundingBox(min, max);
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
