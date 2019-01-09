using OpenSage.Graphics;
using Veldrid;

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

        public static string GetAssetName(object asset)
        {
            switch (asset)
            {
                case Model m: return $"W3DContainer:{m.Name}";
                case Texture t: return $"Texture:{t.Name}";
                default: return null;
            }
        }

        public static AssetView CreateAssetView(DiagnosticViewContext context, object asset)
        {
            switch (asset)
            {
                case Model m: return new ModelView(context, m);
                case Texture t: return new TextureView(context, t);
                default: return null;
            }
        }
    }
}
