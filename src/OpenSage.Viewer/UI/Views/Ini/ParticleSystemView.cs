using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Settings;
using OpenSage.Viewer.Framework;

namespace OpenSage.Viewer.UI.Views.Ini
{
    internal sealed class ParticleSystemView : GameView
    {
        // We need to copy the identity matrix so that we can pass it by reference.
        private static readonly Matrix4x4 WorldIdentity = Matrix4x4.Identity;

        public ParticleSystemView(AssetViewContext context, FXParticleSystemTemplate particleSystemTemplate)
            : base(context)
        {
            var game = context.Game;

            var particleSystem = AddDisposable(new ParticleSystem(
                game.ContentManager,
                particleSystemTemplate,
                () => ref WorldIdentity));

            void onUpdating(object sender, GameUpdatingEventArgs e)
            {
                particleSystem.Update(e.GameTime);
            }

            game.Updating += onUpdating;

            AddDisposeAction(() => game.Updating -= onUpdating);

            void onBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                particleSystem.BuildRenderList(e.RenderList, Matrix4x4.Identity);
            }

            game.BuildingRenderList += onBuildingRenderList;

            AddDisposeAction(() => game.BuildingRenderList += onBuildingRenderList);

            game.Scene3D = new Scene3D(
                game,
                new ArcballCameraController(Vector3.Zero, 200),
                null,
                null,
                Array.Empty<Terrain.WaterArea>(),
                Array.Empty<Terrain.Road>(),
                Array.Empty<Terrain.Bridge>(),
                null,
                new GameObjectCollection(game.ContentManager),
                new WaypointCollection(),
                new WaypointPathCollection(),
                WorldLighting.CreateDefault(),
                Array.Empty<Player>(),
                Array.Empty<Team>());
        }
    }
}
