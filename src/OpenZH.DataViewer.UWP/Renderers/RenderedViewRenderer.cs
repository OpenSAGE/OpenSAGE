using System.ComponentModel;
using OpenZH.DataViewer.Controls;
using OpenZH.DataViewer.UWP.Renderers;
using OpenZH.Graphics.Hosting;
using OpenZH.Graphics.Hosting.Direct3D12;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(RenderedView), typeof(RenderedViewRenderer))]

namespace OpenZH.DataViewer.UWP.Renderers
{
    public sealed class RenderedViewRenderer : ViewRenderer<RenderedView, D3D12SwapChainPanel>
    {
        private D3D12SwapChainPanel _swapChainPanel;

        protected override void OnElementChanged(ElementChangedEventArgs<RenderedView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
            {
                _swapChainPanel = new D3D12SwapChainPanel();

                _swapChainPanel.RedrawsOnTimer = e.NewElement.RedrawsOnTimer;

                _swapChainPanel.GraphicsInitialize += OnGraphicsInitialize;
                _swapChainPanel.GraphicsDraw += OnGraphicsDraw;

                SetNativeControl(_swapChainPanel);
            }
        }

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            Element.Initialize(e.GraphicsDevice, e.SwapChain);
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            Element.Draw(e.GraphicsDevice, e.SwapChain);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(RenderedView.RedrawsOnTimer))
            {
                _swapChainPanel.RedrawsOnTimer = Element.RedrawsOnTimer;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_swapChainPanel != null)
            {
                _swapChainPanel.GraphicsDraw -= OnGraphicsDraw;
                _swapChainPanel.GraphicsInitialize -= OnGraphicsInitialize;
            }

            base.Dispose(disposing);
        }
    }
}
