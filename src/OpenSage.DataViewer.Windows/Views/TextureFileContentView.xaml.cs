using System.Windows.Controls;
using LLGfx.Hosting;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Views
{
    public partial class TextureFileContentView : UserControl
    {
        public TextureFileContentView()
        {
            InitializeComponent();
        }

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            ((TextureFileContentViewModel) DataContext).Initialize(e.GraphicsDevice, e.SwapChain);
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            ((TextureFileContentViewModel) DataContext).Draw(e.GraphicsDevice, e.SwapChain);
        }
    }
}
