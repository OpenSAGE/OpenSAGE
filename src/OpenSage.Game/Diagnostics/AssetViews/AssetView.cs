namespace OpenSage.Diagnostics.AssetViews
{
    internal abstract class AssetView : DisposableBase
    {
        protected DiagnosticViewContext Context { get; }

        protected AssetView(DiagnosticViewContext context)
        {
            Context = context;
        }

        public abstract void Draw();
    }
}
