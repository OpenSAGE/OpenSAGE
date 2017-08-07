using System.Windows.Controls;
using OpenZH.DataViewer.ViewModels;
using OpenZH.Graphics.LowLevel.Hosting;

namespace OpenZH.DataViewer.Views
{
    public partial class RenderedFileContentView : UserControl
    {
        public RenderedFileContentView()
        {
            InitializeComponent();
        }

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            ((RenderedFileContentViewModel) DataContext).Initialize(e.GraphicsDevice, e.SwapChain);
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            ((RenderedFileContentViewModel) DataContext).Draw(e.GraphicsDevice, e.SwapChain);
        }
    }
}
