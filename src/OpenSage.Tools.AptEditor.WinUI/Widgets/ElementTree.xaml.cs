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
using System.Text.Json;

namespace OpenSage.Tools.AptEditor.WinUI.Widgets
{
    
    /// <summary>
    /// ElementList.xaml 的交互逻辑
    /// </summary>
    public partial class ElementTree : DockPanel
    {
        public record Item(int Id, string Name, string Type, int TeeeViewIndex);
        public Item Selected => (Item) Tree.SelectedItem;

        public App TheApp
        {
            get { return (App) GetValue(TheAppProperty); }
            set { SetValue(TheAppProperty, value); }
        }
        public static readonly DependencyProperty TheAppProperty = DependencyProperty.Register(
            "TheApp", typeof(App), typeof(ElementTree),
            new((d, e) => {  })
            );

        public void RefreshTree()
        {
            var ae = TheApp.Editor.Selected;
            if (ae != null)
            {
                Stack<(int, Item)> s = new();
                var sit = new Item(0, "[Root]", "[Root]", Tree.Items.Count);
                s.Push((1, sit));
                Tree.Items.Add(sit);
                while (s.Count > 0)
                {
                    var (nid, nit) = s.Pop();
                    var ntype = ae.GetType(nid).Info;
                    var ndn = ae.GetDisplayName(nid).Info;

                    /*
                    var f = ae.GetFields(nid).Info;
                    Console.WriteLine(nstr + " Fields: " + f);
                    var farr = JsonSerializer.Deserialize<List<string>>(f);
                    foreach (var ff in farr)
                        Console.WriteLine(nstr + $"  {ff}: {Get(nid, ff).Info}");
                    */
                    
                    Item cit = new(nid, ndn, ntype, Tree.Items.Count);
                    // nit.Children.Add(cit);
                    (Tree.ItemContainerGenerator.ContainerFromIndex(sit.TeeeViewIndex) as TreeViewItem).Items.Add(cit);

                    var c = ae.GetChildren(nid).Info;
                    var carr = JsonSerializer.Deserialize<List<int>>(c);
                    carr!.Reverse();
                    foreach (var cc in carr)
                        s.Push((cc, cit));
                }
            }



        }

        public void EditDetails()
        {

        }

        public void EditProperties()
        {

        }

        public ElementTree()
        {
            InitializeComponent();
        }
    }
}
