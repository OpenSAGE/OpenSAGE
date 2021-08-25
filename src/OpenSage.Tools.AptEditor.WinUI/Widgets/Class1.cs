using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using AvalonDock;

namespace OpenSage.Tools.AptEditor.WinUI.Widgets
{
    class Class1 : WidgetBase
    {
        public Class1()
        {

            Children.Add(new TextBox() { Width = 100, Height = 50, Text = "テキストボックス" });
            Children.Add(new Button() { Width = 100, Height = 50, Content = "ボタン" });
        }
    }
}
