using OpenZH.Graphics;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public abstract class RenderedView : View
    {
        public static readonly BindableProperty RedrawsOnTimerProperty = BindableProperty.Create(
            nameof(RedrawsOnTimer), typeof(bool), typeof(RenderedView), defaultValue: false);

        public bool RedrawsOnTimer
        {
            get { return (bool) GetValue(RedrawsOnTimerProperty); }
            set { SetValue(RedrawsOnTimerProperty, value); }
        }

        public abstract void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain);

        public abstract void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain);
    }
}
