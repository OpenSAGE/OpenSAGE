using OpenSage.Gui.Apt;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class AptView : GameView
    {
        public AptView(AssetViewContext context)
            : base(context)
        {
            var aptWindow = context.Game.ContentManager.Load<AptWindow>(context.Entry.FilePath);
            context.Game.Scene2D.AptWindowManager.PushWindow(aptWindow);

            AddDisposeAction(() => context.Game.Scene2D.AptWindowManager.PopWindow());
        }
    }
}
