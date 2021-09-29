using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AvalonDock.Layout;
using OpenSage.Tools.AptEditor;
using OpenSage.Tools.AptEditor.Api;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.WinUI.Widgets
{
    /// <summary>
    /// ElementList.xaml 的交互逻辑
    /// </summary>
    public partial class FileList : DockPanel
    {
        public record Item(int Id, string ShortPath, string FullPath);
        public Item Selected => (Item) List.SelectedItem;

        public App TheApp
        {
            get { return (App) GetValue(TheAppProperty); }
            set { SetValue(TheAppProperty, value); }
        }
        public static readonly DependencyProperty TheAppProperty = DependencyProperty.Register(
            "TheApp", typeof(App), typeof(FileList),
            new((d, e) => { ((FileList) d).RootPath = new(((App) e.NewValue).RootPath); })
            );

        public string RootPath {
            get
            {
                return TheApp == null ? "Null" : TheApp.RootPath;
            }
            set {
                PathTextBox.Text = value;
                TheApp.RootPath = value;
                RefreshList(value);
            } }

        public void RefreshList(string path, int max = 16384)
        {
            var directory = path; // TODO? sanity check
            // void cancel() { token.ThrowIfCancellationRequested(); }
            bool filter(string file) { return file.EndsWith(".apt"); }

            var files = FileUtilities.GetFilesByDirectory(directory, max, filter, null);
            List.Items.Clear();
            foreach (var file in files)
            {
                List.Items.Add(new Item(0, file, System.IO.Path.Combine(directory, file)));
            }
        }

        
        public void OpenNewFile()
        {
        }

        public FileList()
        {
            InitializeComponent();
        }
    }
}
