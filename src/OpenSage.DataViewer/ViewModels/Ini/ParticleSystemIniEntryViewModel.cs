using OpenSage.Data.Ini;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ParticleSystemIniEntryViewModel : FileSubObjectViewModel
    {
        private readonly ParticleSystem _iniParticleSystem;

        public override string GroupName => "Particle Systems";

        public override string Name => _iniParticleSystem.Name;

        public ParticleSystemIniEntryViewModel(ParticleSystem iniParticleSystem)
        {
            _iniParticleSystem = iniParticleSystem;
        }
    }
}
