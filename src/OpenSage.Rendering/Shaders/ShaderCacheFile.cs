using System.IO;
using Veldrid;

namespace OpenSage.Rendering;

internal sealed class ShaderCacheFile
{
    private const int Version = 1;

    public readonly string VsEntryPoint;
    public readonly byte[] VsBytes;

    public readonly string FsEntryPoint;
    public readonly byte[] FsBytes;

    public readonly ResourceLayoutDescription[] ResourceLayoutDescriptions;

    public static bool TryLoad(string filePath, out ShaderCacheFile result)
    {
        if (!File.Exists(filePath))
        {
            result = null;
            return false;
        }

        using var fileStream = File.OpenRead(filePath);
        using var binaryReader = new BinaryReader(fileStream);

        var version = binaryReader.ReadInt32();
        if (version != Version)
        {
            result = null;
            return false;
        }

        var vsEntryPoint = binaryReader.ReadString();

        var vsBytesLength = binaryReader.ReadInt32();
        var vsBytes = new byte[vsBytesLength];
        for (var i = 0; i < vsBytesLength; i++)
        {
            vsBytes[i] = binaryReader.ReadByte();
        }

        var fsEntryPoint = binaryReader.ReadString();

        var fsBytesLength = binaryReader.ReadInt32();
        var fsBytes = new byte[fsBytesLength];
        for (var i = 0; i < fsBytesLength; i++)
        {
            fsBytes[i] = binaryReader.ReadByte();
        }

        var numResourceLayoutDescriptions = binaryReader.ReadInt32();
        var resourceLayoutDescriptions = new ResourceLayoutDescription[numResourceLayoutDescriptions];
        for (var i = 0; i < numResourceLayoutDescriptions; i++)
        {
            var numElements = binaryReader.ReadInt32();

            resourceLayoutDescriptions[i] = new ResourceLayoutDescription
            {
                Elements = new ResourceLayoutElementDescription[numElements]
            };

            for (var j = 0; j < numElements; j++)
            {
                resourceLayoutDescriptions[i].Elements[j] = new ResourceLayoutElementDescription
                {
                    Name = binaryReader.ReadString(),
                    Kind = (ResourceKind)binaryReader.ReadByte(),
                    Stages = (ShaderStages)binaryReader.ReadByte(),
                    Options = (ResourceLayoutElementOptions)binaryReader.ReadByte(),
                };
            }
        }

        result = new ShaderCacheFile(
            vsEntryPoint,
            vsBytes,
            fsEntryPoint,
            fsBytes,
            resourceLayoutDescriptions);

        return true;
    }

    public ShaderCacheFile(
        string vsEntryPoint,
        byte[] vsBytes,
        string fsEntryPoint,
        byte[] fsBytes,
        ResourceLayoutDescription[] resourceLayoutDescriptions)
    {
        VsEntryPoint = vsEntryPoint;
        VsBytes = vsBytes;

        FsEntryPoint = fsEntryPoint;
        FsBytes = fsBytes;

        ResourceLayoutDescriptions = resourceLayoutDescriptions;
    }

    public void Save(string filePath)
    {
        using var fileStream = File.OpenWrite(filePath);
        using var binaryWriter = new BinaryWriter(fileStream);

        binaryWriter.Write(Version);

        binaryWriter.Write(VsEntryPoint);

        binaryWriter.Write(VsBytes.Length);
        foreach (var value in VsBytes)
        {
            binaryWriter.Write(value);
        }

        binaryWriter.Write(FsEntryPoint);

        binaryWriter.Write(FsBytes.Length);
        foreach (var value in FsBytes)
        {
            binaryWriter.Write(value);
        }

        binaryWriter.Write(ResourceLayoutDescriptions.Length);
        for (var i = 0; i < ResourceLayoutDescriptions.Length; i++)
        {
            ref readonly var description = ref ResourceLayoutDescriptions[i];

            binaryWriter.Write(description.Elements.Length);

            for (var j = 0; j < description.Elements.Length; j++)
            {
                ref readonly var element = ref description.Elements[j];

                binaryWriter.Write(element.Name);
                binaryWriter.Write((byte)element.Kind);
                binaryWriter.Write((byte)element.Stages);
                binaryWriter.Write((byte)element.Options);
            }
        }
    }
}
