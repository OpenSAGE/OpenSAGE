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
    public bool IsCliffCell(float x, float y) => false;

    // TODO(Port): Implement this.
    public bool IsUnderwater(float x, float y)
    {
        return false;
    }

    // TODO(Port): Implement this.
    public bool IsUnderwater(float x, float y, out float waterZ)
    {
        waterZ = 0;
        return IsUnderwater(x, y);
    }

    public bool IsUnderwater(float x, float y, out float waterZ, out float terrainZ)
    {
        var result = IsUnderwater(x, y, out waterZ);

        terrainZ = GetGroundHeight(x, y);

        return result;
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

    // TODO(Port): Implement this.
    public float GetLayerHeight(float x, float y, PathfindLayerType layer, out Vector3 normal, bool clip = true)
    {
        normal = Vector3.UnitZ;
        return HeightMap.GetHeight(x, y);
    }

    // TODO(Port): Implement this.
    public float GetLayerHeight(float x, float y, PathfindLayerType layer)
    {
        return HeightMap.GetHeight(x, y);
    }

    // TODO(Port): Implement this.
    public PathfindLayerType GetLayerForDestination(in Vector3 pos) => PathfindLayerType.Ground;

    /// <summary>
    /// This is just like <see cref="GetLayerForDestination"/>, but always
    /// return the highest layer that will be <= z at that point (unlike
    /// <see cref="GetLayerForDestination"/>, which will return the closest
    /// layer).
    /// </summary>
    // TODO(Port): Implement this.
    public PathfindLayerType GetHighestLayerForDestination(in Vector3 pos, bool onlyHealthyBridges = false) => PathfindLayerType.Ground;

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        reader.PersistVersion(2);
        reader.EndObject();

        reader.SkipUnknownBytes(8);
    }
}
