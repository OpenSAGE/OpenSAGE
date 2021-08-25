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

namespace OpenSage.Tools.AptEditor.WinUI.Widgets
{
    /// <summary>
    /// ElementList.xaml 的交互逻辑
    /// </summary>
    public partial class ElementTree : DockPanel
    {
        public ListBoxItem Selected { get; private set; }

        private void OnSelectionChanged(object sender, EventArgs e)
        {

        }

        public ElementTree()
        {
            InitializeComponent();
            // Tree.SelectionChanged += OnSelectionChanged;
        }
    }
}
