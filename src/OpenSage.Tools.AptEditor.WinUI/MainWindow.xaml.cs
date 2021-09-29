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
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Tools.AptEditor.Api;
using OpenSage.Tools.AptEditor.Apt;

namespace OpenSage.Tools.AptEditor.WinUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public App TheApp
        {
            get { return (App) GetValue(TheAppProperty); }
            set { SetValue(TheAppProperty, value); }
        }
        public static readonly DependencyProperty TheAppProperty = DependencyProperty.Register("TheApp", typeof(App), typeof(MainWindow));

        public EditorApi Editor => TheApp.Editor;

        public void HandleExceptionInfo(OperationState s)
        {
            Console.WriteLine(s);
        }

        private void OpenFile(object sender, ExecutedRoutedEventArgs args)
        {
            string path = args.Parameter as string;
            var c = Editor.Open(path);
            if (c.Code == 0)
            {
                var fid = int.Parse(c.Info);
                ChangeSelectedFile(fid);
            }
            else
                HandleExceptionInfo(c);
        }

        private void ChangeSelectedFile(int fid)
        {
            Editor.Select(fid);
            Elements.RefreshTree();
        }

        private void StartEditingProperties(object sender, ExecutedRoutedEventArgs args)
        {

        }

        public MainWindow(App app)
        {
            TheApp = app;

            InitializeComponent();

            Properties.TheApp = TheApp;
        }
    }
}
