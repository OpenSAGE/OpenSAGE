using OpenZH.DataViewer.Helpers;
using System.Collections.ObjectModel;

namespace OpenZH.DataViewer.ViewModels
{
    public abstract class ItemViewModel : ObservableObject
    {
        private bool _createdChildren;

        public ObservableCollection<ItemViewModel> Children { get; } = new ObservableCollection<ItemViewModel>();

        public abstract string DisplayName { get; }
        public abstract string GroupName { get; }

        public void OnSelected()
        {
            if (!_createdChildren)
            {
                CreateChildren();
                _createdChildren = true;
            }
        }

        protected virtual void CreateChildren() { }
    }
}
