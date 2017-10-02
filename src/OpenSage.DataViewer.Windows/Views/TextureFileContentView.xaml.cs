using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LLGfx.Hosting;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Views
{
    public partial class TextureFileContentView : UserControl
    {
        public TextureFileContentView()
        {
            InitializeComponent();

            GraphicsDeviceControl.GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;
        }

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            ((TextureFileContentViewModel) DataContext).Initialize(e.GraphicsDevice, e.SwapChain);
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            ((TextureFileContentViewModel) DataContext).Draw(e.GraphicsDevice, e.SwapChain);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ((TextureFileContentViewModel) DataContext).Dispose();
        }
    }
}
