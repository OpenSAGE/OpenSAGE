using OpenSage.Data;
using OpenSage.Data.Ini;
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

            foreach (var objectDefinition in _iniDataContext.Objects)
            {
                SubObjects.Add(new ObjectDefinitionIniEntryViewModel(objectDefinition));
            }

            foreach (var particleSystem in _iniDataContext.ParticleSystems)
            {
                SubObjects.Add(new ParticleSystemIniEntryViewModel(particleSystem));
            }
        }
    }
}
