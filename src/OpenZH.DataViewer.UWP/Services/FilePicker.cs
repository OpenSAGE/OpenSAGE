using OpenZH.DataViewer.Services;
using Plugin.FilePicker;
using System.Threading.Tasks;

namespace OpenZH.DataViewer.UWP.Services
{
    public class FilePicker : IFilePicker
    {
        public async Task<byte[]> PickFile()
        {
            return (await CrossFilePicker.Current.PickFile()).DataArray;
        }
    }
}
