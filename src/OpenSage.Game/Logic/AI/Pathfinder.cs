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
    /// De-classifies the given object's cells in the map.
    /// </summary>
    /// <param name="gameObject"></param>
    public void RemoveObjectFromPathfindMap(GameObject gameObject)
    {
        // TODO(Port): Implement this.
    }
}
