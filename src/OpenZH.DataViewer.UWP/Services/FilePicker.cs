using System;
using System.Threading.Tasks;
using OpenZH.DataViewer.Services;
using OpenZH.DataViewer.UWP.Util;
using Windows.ApplicationModel.Core;
using Windows.Storage.AccessCache;
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
                    var futureAccessList = StorageApplicationPermissions.FutureAccessList;

                    if (futureAccessList.Entries.Count > 0)
                    {
                        var entry = futureAccessList.Entries[0];
                        var futureFolder = await futureAccessList.GetFolderAsync(entry.Token);
                        return futureFolder.Path;
                    }

                    var folderPicker = new FolderPicker();
                    folderPicker.FileTypeFilter.Add(".something"); // Otherwise PickSingleFolderAsync throws a COMException.

                    var pickedFolder = await folderPicker.PickSingleFolderAsync();

                    futureAccessList.Add(pickedFolder);

                    return pickedFolder.Path;
                });
        }
    }
}
