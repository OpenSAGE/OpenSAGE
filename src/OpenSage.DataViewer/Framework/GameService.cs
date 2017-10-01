using System;
using Caliburn.Micro;
using OpenSage.Data;

namespace OpenSage.DataViewer.Framework
{
    public sealed class GameService : IDisposable
    {
        public Game Game { get; private set; }

        public void OnFileSystemChanged(FileSystem fileSystem)
        {
            Game?.Dispose();

            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            Game = new Game(graphicsDevice, fileSystem);
        }

        public void Dispose()
        {
            if (Game != null)
            {
                Game.Dispose();
                Game = null;
            }
        }
    }
}
