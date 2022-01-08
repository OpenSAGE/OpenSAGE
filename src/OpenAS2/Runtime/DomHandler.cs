using System.IO;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime
{
    /// <summary>
    /// DOM handler
    /// </summary>
    public class DomHandler
    {
        // Delegate to call a function inside the engine
        public delegate void HandleCommand(ExecutionContext context, string command, string param);
        public HandleCommand cmdHandler;

        // Delegate to retrieve an internal variable from the engine
        public delegate Value HandleExternVariable(string variable);
        public HandleExternVariable VariableHandler;

        // Delegate to load another movie
        public delegate AptFile HandleExternalMovie(string movie);
        public HandleExternalMovie movieHandler;

        public ESObject RootObject;
        public DomHandler(HandleCommand hc, HandleExternVariable hev, HandleExternalMovie hem)
        {
            cmdHandler = hc;
            VariableHandler = hev;
            movieHandler = hem;
        }

        public void Handle(ExecutionContext context, string url, string target)
        {
            Logger.Debug($"[URL] URL: {url} Target: {target}");

            if (url.StartsWith("FSCommand:"))
            {
                var command = url.Replace("FSCommand:", "");

                cmdHandler(context, command, target);
            }
            else
            {
                //DO STUFF
                var targetObject = context.This.IGet(target).ToObject<StageObject>();

                if (!(targetObject.Item is SpriteItem))
                {
                    Logger.Error("[URL] Target must be a sprite!");
                }

                var targetSprite = targetObject.Item as SpriteItem;
                var aptFile = movieHandler(url);
                var oldName = targetSprite.Name;

                targetSprite.Create(aptFile.Movie, targetSprite.Context, targetSprite.Parent);
                targetSprite.Name = oldName;
            }
        }

        internal object LoadAptWindowAndQueryPush(string url)
        {
            throw new System.NotImplementedException();
        }
    }
}
