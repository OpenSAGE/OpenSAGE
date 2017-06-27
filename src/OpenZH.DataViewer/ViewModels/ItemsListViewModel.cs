using OpenZH.DataViewer.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace OpenZH.DataViewer.ViewModels
{
    public class ItemsViewModel : ObservableObject
    {
        private IEnumerable<GroupViewModel> _groupedItems;

        public IEnumerable<GroupViewModel> GroupedItems
        {
            get { return _groupedItems; }
            set
            {
                _groupedItems = value;
                OnPropertyChanged();
            }
        }

        public void SetItems(IEnumerable<ItemViewModel> items)
        {
            GroupedItems = items
                .GroupBy(x => x.GroupName)
                .Select(x => new GroupViewModel(x.ToList(), x.Key))
                .ToList();
        }

        public sealed class GroupViewModel : Collection<ItemViewModel>
        {
            public string GroupName { get; }

            public GroupViewModel(IList<ItemViewModel> items, string groupName)
                : base(items)
            {
                GroupName = groupName;
            }
        }
    }
}
