using System.Windows;
using System.Globalization;
using System.Threading;

namespace OpenSage.DataViewer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            base.OnStartup(e);
        }
    }
}
