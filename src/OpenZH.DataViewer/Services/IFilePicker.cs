using System.Threading.Tasks;

namespace OpenZH.DataViewer.Services
{
    public interface IFilePicker
    {
        Task<string> PickFolder();
    }
}
