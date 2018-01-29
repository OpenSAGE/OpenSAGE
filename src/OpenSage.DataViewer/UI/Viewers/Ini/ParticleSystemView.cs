using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.UI.Viewers.Ini
{
    public sealed class ParticleSystemView : GameControl
    {
        public ParticleSystemView(Func<IntPtr, Game> createGame, ParticleSystemDefinition particleSystemDefinition)
        {
            var scene = new Scene();

            var particleSystemEntity = new Entity();
            particleSystemEntity.Components.Add(new ParticleSystem(particleSystemDefinition));
            scene.Entities.Add(particleSystemEntity);

            scene.CameraController = new ArcballCameraController(
                Vector3.Zero,
                200);

            CreateGame = h =>
            {
                var game = createGame(h);

                game.Scene = scene;

                return game;
            };
        }
    }
}
