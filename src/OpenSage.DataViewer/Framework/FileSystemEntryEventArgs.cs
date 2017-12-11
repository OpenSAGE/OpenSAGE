using System;
using OpenSage.Data;

namespace OpenSage.DataViewer.Framework
{
    public sealed class FileSystemEntryEventArgs : EventArgs
    {
        public FileSystemEntry Entry { get; }

        public FileSystemEntryEventArgs(FileSystemEntry entry)
        {
            Entry = entry;
        }
    }
}
