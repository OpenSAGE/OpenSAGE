using System.Diagnostics;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// Url handler
    /// </summary>
    public class UrlHandler
    {
        public void Handle(string url, string target)
        {
            Debug.WriteLine("[URL] URL: " + url + " Target: " + target);

        }
    }
}
