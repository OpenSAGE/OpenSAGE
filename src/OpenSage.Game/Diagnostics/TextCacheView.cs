namespace OpenSage.Diagnostics {
    internal sealed class TextCacheView : DiagnosticView
    {
        public TextCacheView(DiagnosticViewContext context) : base(context)
        {
        }

        public override string DisplayName => "Text Cache";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            var newSelectedObject = Context.SelectedObject;
            Context.Game.Graphics.RenderPipeline.DrawingContext.TextCache.DrawDiagnostic(ref newSelectedObject);
            if (newSelectedObject != Context.SelectedObject)
            {
                Context.SelectedObject = newSelectedObject;
            }
        }
    }
}
