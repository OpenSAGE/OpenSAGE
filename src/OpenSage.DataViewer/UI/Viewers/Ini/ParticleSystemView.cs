using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic.Object;
using OpenSage.Settings;

namespace OpenSage.DataViewer.UI.Viewers.Ini
{
    public sealed class ParticleSystemView : GameControl
    {
        public ParticleSystemView(Func<IntPtr, Game> createGame, ParticleSystemDefinition particleSystemDefinition)
        {
            CreateGame = h =>
            {
                var game = createGame(h);

                var particleSystem = new ParticleSystem(
                    game.ContentManager,
                    particleSystemDefinition,
                    () => Matrix4x4.Identity);

                game.Updating += (sender, e) =>
                {
                    particleSystem.Update(e.GameTime);
                };

                game.BuildingRenderList += (sender, e) =>
                {
                    particleSystem.BuildRenderList(e.RenderList, Matrix4x4.Identity);
                };

                game.Scene3D = new Scene3D(
                    game,
                    new ArcballCameraController(Vector3.Zero, 200),
                    null,
                    null,
                    null,
                    new GameObjectCollection(game.ContentManager),
                    new WaypointCollection(),
                    new WaypointPathCollection(),
                    WorldLighting.CreateDefault());

                return game;
            };
        }
    }
}
