using System.Numerics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Diagnostics.AssetViews
{
    [AssetView(typeof(FXParticleSystemTemplate))]
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
                particleSystemTemplate,
                game.AssetStore.LoadContext,
                () => ref WorldIdentity));

            _renderedView = AddDisposable(new RenderedView(context));

            void onBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                particleSystem.BuildRenderList(e.RenderList, e.GameTime);
            }

            _renderedView.RenderPipeline.BuildingRenderList += onBuildingRenderList;

            AddDisposeAction(() => _renderedView.RenderPipeline.BuildingRenderList -= onBuildingRenderList);
        }

        public override void Draw()
        {
            _renderedView.Draw();
        }
    }
}
