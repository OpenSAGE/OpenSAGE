namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// Url handler
    /// </summary>
    public static class UrlHandler
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Handle(VM.HandleCommand handler, ActionContext context, string url, string target)
        {
            logger.Debug($"[URL] URL: {url} Target: {target}");

            if (url.StartsWith("FSCommand:"))
            {
                var command = url.Replace("FSCommand:", "");

                handler(context, command, target);
            }
        }
    }
}
