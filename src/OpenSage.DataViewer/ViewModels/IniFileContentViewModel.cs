using System.Collections.Generic;
using Caliburn.Micro;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels.Ini;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class IniFileContentViewModel : FileContentViewModel<FileSubObjectViewModel>
    {
        private readonly IniDataContext _iniDataContext;

        private readonly ParticleSystemManager _particleSystemManager;
        private readonly ContentManager _contentManager;

        public IniFileContentViewModel(FileSystemEntry file) 
            : base(file)
        {
            _iniDataContext = new IniDataContext();
            _iniDataContext.LoadIniFile(file);

            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            _particleSystemManager = AddDisposable(new ParticleSystemManager(graphicsDevice));

            _contentManager = AddDisposable(new ContentManager(file.FileSystem, graphicsDevice));
        }

        protected override IReadOnlyList<FileSubObjectViewModel> CreateSubObjects()
        {
            var result = new List<FileSubObjectViewModel>();

            //foreach (var objectDefinition in _iniDataContext.Objects)
            //{
            //    result.Add(new IniEntryViewModel(
            //        objectDefinition,
            //        "Object Definitions",
            //        objectDefinition.Name));
            //}

            foreach (var particleSystem in _iniDataContext.ParticleSystems)
            {
                result.Add(new ParticleSystemIniEntryViewModel(particleSystem, _particleSystemManager, _contentManager));
            }

            return result;
        }
    }
}
