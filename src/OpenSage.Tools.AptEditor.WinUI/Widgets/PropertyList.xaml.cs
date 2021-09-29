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
    public partial class PropertyList : DockPanel
    {
        public record Item(string Field, string Value, string Type);
        public Item Selected => (Item) List.SelectedItem;

        public App TheApp { get; set; }

        public string Path { get; set; }
        public void RefreshList()
        {

        }

        public void OpenNewFile()
        {

        }

        public PropertyList()
        {
            InitializeComponent();
        }
    }
}
