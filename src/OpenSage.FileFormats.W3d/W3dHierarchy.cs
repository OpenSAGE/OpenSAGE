using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

/// <summary>
/// WHT ( Westwood Hierarchy Tree )
/// A hierarchy tree defines a set of coordinate systems which are connected
/// hierarchically. The header defines the name, number of pivots, etc.
/// The pivots chunk will contain a W3dPivotStructs for each node in the
/// tree.
///
/// The W3dPivotFixupStruct contains a transform for each MAX coordinate
/// system and our version of that same coordinate system (bone). It is
/// needed when the user exports the base pose using "Translation Only".
/// These are the matrices which go from the MAX rotated coordinate systems
/// to a system which is unrotated in the base pose .These transformations
/// are needed when exporting a hierarchy animation with the given hierarchy
/// tree file.
///
/// Another explanation of these kludgy "fixup" matrices:
///
/// What are the "fixup" matrices? These are the transforms which
/// were applied to the base pose when the user wanted to force the
/// base pose to use only matrices with certain properties. For
/// example, if we wanted the base pose to use translations only,
/// the fixup transform for each node is a transform which when
/// multiplied by the real node's world transform, yeilds a pure
/// translation matrix. Fixup matrices are used in the mesh
/// exporter since all vertices must be transformed by their inverses
/// in order to make things work. They also show up in the animation
/// exporter because they are needed to make the animation work with
/// the new base pose.
/// </summary>
/// <param name="Version"></param>
/// <param name="Name">Name of the hierarchy</param>
/// <param name="NumPivots"></param>
/// <param name="Center"></param>
public sealed record W3dHierarchy(uint Version, string Name, uint NumPivots, Vector3 Center)
    : W3dChunk(W3dChunkType.W3D_CHUNK_HIERARCHY_HEADER)
{
    internal static W3dHierarchy Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var numPivots = reader.ReadUInt32();
            var center = reader.ReadVector3();

            return new W3dHierarchy(version, name, numPivots, center);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.Write(NumPivots);
        writer.Write(Center);
    }
}
