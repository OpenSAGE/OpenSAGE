using System.Collections.Generic;
using Caliburn.Micro;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels.Ini;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class IniFileContentViewModel : FileContentViewModel<FileSubObjectViewModel>
    {
        private readonly IniDataContext _iniDataContext;

        private readonly GameContext _gameContext;

        public IniFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _iniDataContext = new IniDataContext();
            _iniDataContext.LoadIniFile(file);

            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            _gameContext = AddDisposable(new GameContext(
                file.FileSystem, 
                graphicsDevice));
        }

        protected override IReadOnlyList<FileSubObjectViewModel> CreateSubObjects()
        {
            var result = new List<FileSubObjectViewModel>();

            foreach (var objectDefinition in _iniDataContext.Objects)
            {
                result.Add(new ObjectDefinitionIniEntryViewModel(
                    objectDefinition,
                    _gameContext));
            }

            foreach (var particleSystem in _iniDataContext.ParticleSystems)
            {
                result.Add(new ParticleSystemIniEntryViewModel(
                    particleSystem,
                    _gameContext.ContentManager));
            }

            return result;
        }
    }
}
