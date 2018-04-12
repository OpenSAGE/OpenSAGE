namespace OpenSage.Viewer.UI.Views
{
    internal sealed class MapView : GameView
    {
        public MapView(AssetViewContext context)
            : base(context)
        {
            context.Game.Scene3D = context.Game.ContentManager.Load<Scene3D>(context.Entry.FilePath);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            // TODO

            base.Draw(ref isGameViewFocused);
        }
    }
}
