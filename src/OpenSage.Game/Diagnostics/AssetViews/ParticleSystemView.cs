using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Settings;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class ParticleSystemView : AssetView
    {
        // We need to copy the identity matrix so that we can pass it by reference.
        private static readonly Matrix4x4 WorldIdentity = Matrix4x4.Identity;

        private readonly RenderedView _renderedView;

        public ParticleSystemView(DiagnosticViewContext context, FXParticleSystemTemplate particleSystemTemplate)
            : base(context)
        {
            var game = context.Game;

            var particleSystem = AddDisposable(new ParticleSystem(
                game.ContentManager,
                particleSystemTemplate,
                () => ref WorldIdentity));

            void OnUpdating(object sender, GameUpdatingEventArgs e)
            {
                particleSystem.Update(e.GameTime);
            }

            game.Updating += OnUpdating;

            AddDisposeAction(() => game.Updating -= OnUpdating);

            void onBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                particleSystem.BuildRenderList(e.RenderList, Matrix4x4.Identity);
            }

            game.BuildingRenderList += onBuildingRenderList;

            AddDisposeAction(() => game.BuildingRenderList -= onBuildingRenderList);

            var scene3D = new Scene3D(
                game,
                () => new Veldrid.Viewport(0, 0, ImGui.GetContentRegionAvailWidth(), ImGui.GetContentRegionAvail().Y, 0, 1),
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

            _renderedView = AddDisposable(new RenderedView(context, scene3D));
        }

        public override void Draw()
        {
            _renderedView.Draw();
        }
    }
}
