using System;
using OpenSage.Data;

namespace OpenSage.DataViewer.Framework
{
    public sealed class InstallationChangedEventArgs : EventArgs
    {
        public GameInstallation Installation { get; }
        public FileSystem FileSystem { get; }

        public InstallationChangedEventArgs(GameInstallation installation, FileSystem fileSystem)
        {
            Installation = installation;
            FileSystem = fileSystem;
        }
    }
}
