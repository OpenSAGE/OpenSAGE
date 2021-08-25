using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenSage.Tools.AptEditor.WinUI
{
    public partial class PathSelector: Window
    {
        private TextBox tb_path = null;
        private App _app;
        public MainWindow MainWindow { get; private set; }

        public void CallDialog(object sender, EventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    tb_path.Text = dialog.SelectedPath;
            }
        }

        public PathSelector(App app, MainWindow mwin = null, string path = null)
        {
            ResizeMode = ResizeMode.NoResize;
            Height = 180;
            Width = 400;
            _app = app;

            var panel = new DockPanel() { Margin = new Thickness(10) };
            var tb = new TextBlock()
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
                Text = "Start OpenSage Apt Editor by providing a root path.\nIt's recommended that you set it to the RA3SDK_UI_ScreensPack folder so you could load apt files more easily.", 
            };
            tb_path = new TextBox() { Width = 320, Height = 25, Text = path == null ? "" : path };
            var btn_sel = new Button() { Content = "...", Width = 40, Height = 25 };
            var btn_strt = new Button() { Content = "Select Path", Height = 25 };
            btn_sel.Click += CallDialog;
            btn_strt.Click += LaunchProgram;

            panel.Children.Add(tb);
            panel.Children.Add(btn_strt);
            panel.Children.Add(tb_path);
            panel.Children.Add(btn_sel);
            
            DockPanel.SetDock(tb, Dock.Top);
            DockPanel.SetDock(tb_path, Dock.Left);
            DockPanel.SetDock(btn_sel, Dock.Right);
            DockPanel.SetDock(btn_strt, Dock.Bottom);
            Content = panel;

        }

        public void LaunchProgram(object sender, EventArgs e)
        {
            MainWindow = new MainWindow(_app);
            MainWindow.Show();
            Close();
        }

    }
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private string _rootPath = "";
        private PathSelector _pathSelector;
        public string RootPath
        {
            get { return _rootPath; }
            set { _rootPath = value; }
        }

        public void LoadConfig()
        {
            // TODO
            _rootPath = "G:\\Games\\RA#s\\RA3_MODSDK\\aptui";
        }

        public void ChangeRootPath()
        {
            if (_pathSelector == null)
                _pathSelector = new PathSelector(this, null, _rootPath);
            _pathSelector.Show();
        }
        
        public App()
        {
            LoadConfig();
            ChangeRootPath();
        }
    }
}
