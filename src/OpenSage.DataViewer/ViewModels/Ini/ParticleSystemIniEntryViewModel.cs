using System.Numerics;
using Caliburn.Micro;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ParticleSystemIniEntryViewModel : FileSubObjectViewModel, IGameViewModel
    {
        private readonly ParticleSystemDefinition _definition;

        private Entity _particleSystemEntity;
        private ParticleSystem _particleSystem;

        public Game Game { get; }

        public override string GroupName => "Particle Systems";

        public override string Name => _definition.Name;

        public ParticleSystemIniEntryViewModel(
            ParticleSystemDefinition definition,
            Game game)
        {
            _definition = definition;

            Game = game;
        }

        public override void Activate()
        {
            var scene = new Scene();

            var cameraEntity = new Entity();
            scene.Entities.Add(cameraEntity);

            cameraEntity.Components.Add(new PerspectiveCameraComponent
            {
                FieldOfView = 70
            });

            var cameraController = new ArcballCameraController();
            cameraEntity.Components.Add(cameraController);
            cameraController.Reset(Vector3.Zero, 200);

            _particleSystemEntity = new Entity();
            scene.Entities.Add(_particleSystemEntity);

            _particleSystem = new ParticleSystem(_definition);
            _particleSystemEntity.Components.Add(_particleSystem);

            Game.Scene = scene;

            Game.ResetElapsedTime();
        }

        public override void Deactivate()
        {
            _particleSystemEntity.Components.Remove(_particleSystem);
            _particleSystem = null;

            Game.Scene = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (_particleSystem != null)
            {
                Deactivate();
            }

            base.Dispose(disposing);
        }
    }
}
