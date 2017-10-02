using System.Collections.Generic;
using Caliburn.Micro;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels.Ini;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class IniFileContentViewModel : FileContentViewModel<IniEntryViewModel>
    {
        private readonly IniDataContext _iniDataContext;

        public IniFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _iniDataContext = new IniDataContext();
            _iniDataContext.LoadIniFile(file);
        }

        protected override IReadOnlyList<IniEntryViewModel> CreateSubObjects()
        {
            var result = new List<IniEntryViewModel>();

            foreach (var objectDefinition in _iniDataContext.Objects)
            {
                result.Add(new ObjectDefinitionIniEntryViewModel(objectDefinition));
            }

            foreach (var particleSystem in _iniDataContext.ParticleSystems)
            {
                result.Add(new ParticleSystemIniEntryViewModel(particleSystem));
            }

            return result;
        }
    }
}
