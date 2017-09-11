using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LLGfx.Hosting;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Views
{
    public partial class MapFileContentView : UserControl
    {
        public MapFileContentView()
        {
            InitializeComponent();

            GraphicsDeviceControl.GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;
        }

        private MapFileContentViewModel TypedDataContext => (MapFileContentViewModel) DataContext;

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            TypedDataContext.Dispose();
        }

        private void OnGraphicsInitialize(object sender, GraphicsEventArgs e)
        {
            TypedDataContext.Initialize(e.GraphicsDevice, e.SwapChain);
        }

        private void OnGraphicsDraw(object sender, GraphicsEventArgs e)
        {
            TypedDataContext.Draw(e.GraphicsDevice, e.SwapChain);
        }
    }
}
