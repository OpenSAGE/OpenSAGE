using System.Collections.Generic;
using Caliburn.Micro;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels.Ini;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class IniFileContentViewModel : FileContentViewModel<FileSubObjectViewModel>
    {
        private readonly IniDataContext _iniDataContext;
        private readonly Game _game;

        public IniFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _iniDataContext = new IniDataContext();
            _iniDataContext.LoadIniFile(file);

            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;
            _game = new Game(graphicsDevice, file.FileSystem);
        }

        protected override IReadOnlyList<FileSubObjectViewModel> CreateSubObjects()
        {
            var result = new List<FileSubObjectViewModel>();

            foreach (var objectDefinition in _iniDataContext.Objects)
            {
                result.Add(AddDisposable(new ObjectDefinitionIniEntryViewModel(
                    objectDefinition,
                    _game)));
            }

            foreach (var particleSystem in _iniDataContext.ParticleSystems)
            {
                result.Add(AddDisposable(new ParticleSystemIniEntryViewModel(
                    particleSystem,
                    _game)));
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            _game.Scene = null;

            base.Dispose(disposing);
        }
    }
}
