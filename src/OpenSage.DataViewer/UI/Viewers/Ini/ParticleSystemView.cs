using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.UI.Viewers.Ini
{
    public sealed class ParticleSystemView : GameControl
    {
        public ParticleSystemView(Game game, ParticleSystemDefinition particleSystemDefinition)
        {
            var scene = new Scene();

            var particleSystemEntity = new Entity();
            particleSystemEntity.Components.Add(new ParticleSystem(particleSystemDefinition));
            scene.Entities.Add(particleSystemEntity);

            scene.CameraController = new ArcballCameraController(
                Vector3.Zero,
                200);

            game.Scene = scene;

            Game = game;
        }
    }
}
