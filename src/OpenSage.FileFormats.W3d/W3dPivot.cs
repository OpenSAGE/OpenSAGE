using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

/// <param name="Name">Name of the node (UR_ARM, LR_LEG, TORSO, etc)</param>
/// <param name="ParentIdx">0xffffffff = root pivot; no parent</param>
/// <param name="Translation">translation to pivot point</param>
/// <param name="EulerAngles">orientation of the pivot point</param>
/// <param name="Rotation">orientation of the pivot point</param>
public sealed record W3dPivot(
    string Name,
    int ParentIdx,
    Vector3 Translation,
    Vector3 EulerAngles,
    Quaternion Rotation)
{
    internal static W3dPivot Parse(BinaryReader reader)
    {
        var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
        var parentIdx = reader.ReadInt32();
        var translation = reader.ReadVector3();
        var eulerAngles = reader.ReadVector3();
        var rotation = reader.ReadQuaternion();

        return new W3dPivot(name, parentIdx, translation, eulerAngles, rotation);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.Write(ParentIdx);
        writer.Write(Translation);
        writer.Write(EulerAngles);
        writer.Write(Rotation);
    }
}
