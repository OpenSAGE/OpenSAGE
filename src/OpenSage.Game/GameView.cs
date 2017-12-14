using System;
using OpenSage.LowLevel;

namespace OpenSage
{
    public sealed class GameView : HostView
    {
        public Game Game { get; set; }

        protected override void OnGraphicsInitialized(EventArgs args)
        {
            Game.HostView = this;
        }

        protected override void OnGraphicsDraw(EventArgs args)
        {
            if (Game.Scene != null)
            {
                Game.Tick();
            }
        }

        protected override void OnGraphicsResized(EventArgs args)
        {
            Game.SetSwapChain(SwapChain);
        }

        protected override void OnGraphicsUninitialized(EventArgs args)
        {
            Game.HostView = null;
        }
    }
}
