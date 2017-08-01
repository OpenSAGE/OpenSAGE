using Caliburn.Micro;

namespace OpenZH.DataViewer.ViewModels
{
    public class TabViewModel : PropertyChangedBase
    {
        public string Name { get; }

        public TabViewModel(string name)
        {
            Name = name;
        }
    }
}
