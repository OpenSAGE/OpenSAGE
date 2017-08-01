using Caliburn.Micro;

namespace OpenZH.DataViewer.ViewModels
{
    public class ShellViewModel : PropertyChangedBase
    {
        public BindableCollection<TabViewModel> Tabs { get; }

        public ShellViewModel()
        {
            Tabs = new BindableCollection<TabViewModel>
            {
                new TabViewModel("3D Models"),
                new TabViewModel("Textures"),
                new TabViewModel("Maps"),
                new TabViewModel("Other")
            };
        }
    }
}
