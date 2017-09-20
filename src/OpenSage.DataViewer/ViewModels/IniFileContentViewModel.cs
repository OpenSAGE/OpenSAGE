using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Data.Ini;

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
                result.Add(new IniEntryViewModel(
                    objectDefinition,
                    "Object Definitions",
                    objectDefinition.Name));
            }

            foreach (var particleSystem in _iniDataContext.ParticleSystems)
            {
                result.Add(new IniEntryViewModel(
                    particleSystem,
                    "Particle Systems",
                    particleSystem.Name));
            }

            return result;
        }
    }

    public sealed class IniEntryViewModel : FileSubObjectViewModel
    {
        public override string GroupName { get; }

        public override string Name { get; }

        public object IniEntry { get; }

        public IniEntryViewModel(object iniEntry, string groupName, string name)
        {
            IniEntry = iniEntry;
            GroupName = groupName;
            Name = name;
        }
    }
}
