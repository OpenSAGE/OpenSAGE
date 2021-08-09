using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using AvalonDock;
using OpenSage.Tools.AptEditor.UI.NewWidgets;

namespace OpenSage.Tools.AptEditor.UI
{
    class MainWindow: Window
    {
        public MainWindow(): base()
        {
            var g = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 720,
                Margin = new Thickness
                {
                    Bottom = 0,
                    Left = 0,
                    Top = 0,
                    Right = 0
                },
                VerticalAlignment = VerticalAlignment.Top,
                Width = 1280,

            };
            var mgr = new DockingManager();
            var lroot = new LayoutRoot();
            var lpanel = new LayoutPanel();
            
            var dpane = new LayoutDocumentPane();


            dpane.Children.Add(new Class1());
            dpane.Children.Add(new Class1());
            dpane.Children.Add(new Class1());
            dpane.Children.Add(new Class1());
            lpanel.Children.Add(dpane);
            // lroot.Children.Add(lpanel);
            lroot.RootPanel = lpanel;
            mgr.Layout = lroot;
            g.Children.Add(mgr);

            Content = g;
        }
    }
}
