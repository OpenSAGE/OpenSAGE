using System.Collections.ObjectModel;
using Caliburn.Micro;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public abstract class FileContentViewModelBase : PropertyChangedBase
    {

    }

    public abstract class FileContentViewModel : FileContentViewModelBase
    {
        public FileSystemEntry File { get; }

        protected FileContentViewModel(FileSystemEntry file)
        {
            File = file;
        }
    }

    public abstract class FileContentViewModel<TSubObject> : FileContentViewModel
        where TSubObject : FileSubObjectViewModel
    {
        public ObservableCollection<TSubObject> SubObjects { get; } = new ObservableCollection<TSubObject>();

        private TSubObject _selectedSubObject;

        public TSubObject SelectedSubObject
        {
            get { return _selectedSubObject; }
            set
            {
                _selectedSubObject?.Deactivate();

                _selectedSubObject = value;

                _selectedSubObject?.Activate();

                OnSelectedSubObjectChanged(_selectedSubObject);

                NotifyOfPropertyChange();
            }
        }

        protected virtual void OnSelectedSubObjectChanged(TSubObject subObject) { }

        protected FileContentViewModel(FileSystemEntry file) 
            : base(file)
        {
        }
    }

    public abstract class FileSubObjectViewModel : FileContentViewModelBase
    {
        public abstract string GroupName { get; }
        public abstract string Name { get; }

        public virtual void Activate() { }

        public virtual void Deactivate() { }
    }
}
