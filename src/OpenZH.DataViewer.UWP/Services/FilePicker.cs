using System;
using System.Threading.Tasks;
using OpenZH.DataViewer.Services;
using OpenZH.DataViewer.UWP.Util;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;

namespace OpenZH.DataViewer.UWP.Services
{
    public class FilePicker : IFilePicker
    {
        public Task<string> PickFolder()
        {
            return CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(
                async () =>
                {
                    var folderPicker = new FolderPicker();
                    folderPicker.FileTypeFilter.Add(".something"); // Otherwise PickSingleFolderAsync throws a COMException.

                    var pickedFolder = await folderPicker.PickSingleFolderAsync();

                    return pickedFolder.Path;
                });
        }
    }
}
