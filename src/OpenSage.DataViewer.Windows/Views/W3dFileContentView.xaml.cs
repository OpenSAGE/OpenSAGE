using System.Windows.Controls;
using OpenSage.DataViewer.ViewModels;
using LLGfx.Hosting;

namespace OpenSage.DataViewer.Views
{
    public partial class W3dFileContentView : UserControl
    {
        public W3dFileContentView()
        {
            InitializeComponent();
        }

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            ((W3dFileContentViewModel) DataContext).Initialize(e.GraphicsDevice, e.SwapChain);
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            ((W3dFileContentViewModel) DataContext).Draw(e.GraphicsDevice, e.SwapChain);
        }
    }
}
