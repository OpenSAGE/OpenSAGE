using System.Diagnostics;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// Url handler
    /// </summary>
    public static class UrlHandler
    {
        public static void Handle(VM.HandleCommand handler, ActionContext context, string url, string target)
        {
            Debug.WriteLine("[URL] URL: " + url + " Target: " + target);

            if (url.StartsWith("FSCommand:"))
            {
                var command = url.Replace("FSCommand:", "");

                handler(context, command, target);
            }
        }
    }
}
