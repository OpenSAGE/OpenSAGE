using System.Numerics;
using OpenSage.FX;

namespace OpenSage.Diagnostics.AssetViews
{
    [AssetView(typeof(FXList))]
    internal sealed class FXListView : AssetView
    {
        private readonly RenderedView _renderedView;

        public FXListView(DiagnosticViewContext context, FXList fxList)
            : base(context)
        {
            _renderedView = AddDisposable(new RenderedView(context));

            fxList.Execute(
                new FXListExecutionContext(
                    null, // TODO
                    Matrix4x4.Identity,
                    _renderedView.Scene.GameContext));
        }

        public override void Draw()
        {
            _renderedView.Draw();
        }
    }
}
