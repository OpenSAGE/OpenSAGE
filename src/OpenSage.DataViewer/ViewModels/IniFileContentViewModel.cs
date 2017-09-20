using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.ViewModels.Ini;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class IniFileContentViewModel : FileContentViewModel<FileSubObjectViewModel>
    {
        private readonly IniDataContext _iniDataContext;

        public IniFileContentViewModel(FileSystemEntry file) 
            : base(file)
        {
            _iniDataContext = new IniDataContext();
            _iniDataContext.LoadIniFile(file);
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
                result.Add(new ParticleSystemIniEntryViewModel(particleSystem));
            }

            return result;
        }
    }
}
