using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using OpenSage.FileFormats.Apt;
using OpenSage.Tools.AptEditor;
using OpenSage.Tools.AptEditor.Apt;

namespace OpenSage.Tools.AptEditor.WinUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public App App { get; private set; }
        public AptEditInstance CurrentApt { get; private set; }
        public MainWindow(App app)
        {
            App = app;

            InitializeComponent();
        }
    }
}
