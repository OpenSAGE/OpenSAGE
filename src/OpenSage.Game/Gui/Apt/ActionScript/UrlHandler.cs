using System.Diagnostics;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// Url handler
    /// </summary>
    public class UrlHandler
    {
        public void Handle(VM.HandleCommand handler, string url, string target)
        {
            Debug.WriteLine("[URL] URL: " + url + " Target: " + target);


            if(url.StartsWith("FSCommand:"))
            {
                var command = url.Replace("FSCommand:","");
                
                handler(command);
            }
        }
    }
}
