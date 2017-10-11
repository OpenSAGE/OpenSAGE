using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ParticleSystemIniEntryViewModel : IniEntryViewModel, IGameViewModel
    {
        private readonly ParticleSystemDefinition _definition;

        public override string GroupName => "Particle Systems";

        public override string Name => _definition.Name;

        public ParticleSystemIniEntryViewModel(ParticleSystemDefinition definition)
        {
            _definition = definition;
        }

        void IGameViewModel.LoadScene(Game game)
        {
            var scene = new Scene();

            var particleSystemEntity = new Entity();
            particleSystemEntity.Components.Add(new ParticleSystem(_definition));
            scene.Entities.Add(particleSystemEntity);

            game.Scene = scene;
        }
    }
}
