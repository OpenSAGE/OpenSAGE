﻿namespace OpenSage.IO;

public sealed class FileSystemEntry
{
    private readonly Func<Stream> _open;

    public FileSystem FileSystem { get; }
    public string FilePath { get; }
    public uint Length { get; }

    public FileSystemEntry(FileSystem fileSystem, string filePath, uint length, Func<Stream> open)
    {
        FileSystem = fileSystem;
        FilePath = filePath;
        Length = length;
        _open = open;
    }

    public Stream Open() => _open();

    public override string ToString()
    {
        return FilePath;
    }
}
