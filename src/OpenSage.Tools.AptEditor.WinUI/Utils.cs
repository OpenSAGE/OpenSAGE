using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSage.Tools.AptEditor.WinUI
{
    public static class Commands
    {
        public static RoutedCommand OpenNewFileCmd = new RoutedCommand();
        public static RoutedCommand EditElementCmd = new RoutedCommand();
        public static RoutedCommand EditElementInDetailCmd = new RoutedCommand();
    }
}
