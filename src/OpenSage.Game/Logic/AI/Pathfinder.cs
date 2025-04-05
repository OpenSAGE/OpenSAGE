using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI;

public sealed class Pathfinder : BaseSingletonAsset
{
    internal static void Parse(IniParser parser, Pathfinder value) => parser.ParseBlockContent(value, FieldParseTable);

    private static readonly IniParseTable<Pathfinder> FieldParseTable = new IniParseTable<Pathfinder>
    {
        { "SlopeLimits", (parser, x) => x.SlopeLimits = parser.ParseFloatArray() },
    };

    [AddedIn(SageGame.Bfme)]
    public float[] SlopeLimits { get; private set; }

    /// <summary>
    /// Returns true if the given position is a valid movement location for the given layer and locomotor.
    /// </summary>
    public bool ValidMovementTerrain(PathfindLayerType layer, Locomotor locomotor, in Vector3 pos)
    {
        // TODO(Port): Implement this.
        return true;
    }

    /// <summary>
    /// Classifies the given object's cells in the map.
    /// </summary>
    public void AddObjectToPathfindMap(GameObject gameObject)
    {
        // TODO(Port): Implement this.
    }

    /// <summary>
    /// De-classifies the given object's cells in the map.
    /// </summary>
    public void RemoveObjectFromPathfindMap(GameObject gameObject)
    {
        // TODO(Port): Implement this.
    }

    // TEMPORARY STUBS
    internal bool IsLinePassable(GameObject obj, Surfaces acceptableSurfaces, PathfindLayerType layer, Vector3 position1, /* ref ?! */ Vector3 position2, bool blocked, bool v)
    {
        throw new NotImplementedException();
    }

    internal AIPathfind.PathfindCell GetCell(PathfindLayerType layer, Vector3 position)
    {
        throw new NotImplementedException();
    }

    internal bool IsGroundPathPassable(bool crusher, Vector3 position1, PathfindLayerType layer, Vector3 position2, int pathDiameter)
    {
        throw new NotImplementedException();
    }

    internal void SetDebugPathPosition(Vector3 positionOnPath)
    {
        throw new NotImplementedException();
    }
}
