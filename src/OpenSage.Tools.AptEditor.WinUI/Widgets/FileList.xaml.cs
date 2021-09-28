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
using System.Windows.Shapes;
using AvalonDock.Layout;
using OpenSage.Tools.AptEditor;
using OpenSage.Tools.AptEditor.Api;

namespace OpenSage.Tools.AptEditor.WinUI.Widgets
{
    /// <summary>
    /// ElementList.xaml 的交互逻辑
    /// </summary>
    public partial class FileList : DockPanel
    {
        public record Item(int Id, string ShortPath, string FullPath);
        public Item Selected => (Item) List.SelectedItem;


        public App App { get; set; }

        public string Path { get; set; }
        public void RefreshList(object sender, TextChangedEventArgs args)
        {

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
