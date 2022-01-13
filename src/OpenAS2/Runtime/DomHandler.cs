using System.IO;
using System.Linq;
using OpenAS2.Base;
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

        public ESObject RootScriptObject;

        public DomHandler(HandleCommand hc, HandleExternVariable hev, HandleExternalMovie hem)
        {
            cmdHandler = hc;
            VariableHandler = hev;
            movieHandler = hem;
        }

        public bool TryHandle(ExecutionContext context, RawInstruction inst)
        {
            switch (inst.Type)
            {
                case InstructionType.NextFrame:
                    break;
                case InstructionType.PrevFrame:
                    return false; // TODO
                case InstructionType.Play:
                    break;
                case InstructionType.Stop:
                    break;


                case InstructionType.ToggleQuality:
                    return false; // TODO


                case InstructionType.StopSounds:
                    return false; // TODO


                case InstructionType.SetTarget:
                    return false; // TODO
                case InstructionType.SetTarget2:
                    return false; // TODO
                case InstructionType.TargetPath:
                    return false; // TODO


                case InstructionType.CloneSprite:
                    break;
                case InstructionType.RemoveSprite:
                    break;


                case InstructionType.StartDragMovie:
                    return false; // TODO
                case InstructionType.StopDragMovie:
                    return false; // TODO


                case InstructionType.GotoLabel:
                    break;
                case InstructionType.GotoFrame:
                    break;
                case InstructionType.GotoFrame2:
                    break;
                case InstructionType.CallFrame:
                    return false; // TODO
                case InstructionType.WaitFormFrame:
                    return false; // TODO
                case InstructionType.WaitForFrameExpr:
                    return false; // TODO


                case InstructionType.GetURL:
                    break;
                case InstructionType.GetURL2:
                    break;


                case InstructionType.GetTime:
                    break;


                case InstructionType.Trace:
                    break;
                case InstructionType.TraceStart:
                    return false; // TODO


                case InstructionType.GetProperty:
                    break;
                case InstructionType.SetProperty:
                    break;


                case InstructionType.GetVariable:
                    break;
                case InstructionType.SetVariable:
                    break;

                default:
                    return false; // TODO
            }
            return false;
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
                var targetObject = context.This.IGet(target).ToObject<HostObject>();

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

        public Value GetTarget(string target) // TODO
        {
            // not a host object
            if (target.Length == 0 || target.First() != '/')
                return Value.Undefined();

            ESObject obj = RootScriptObject;

            foreach (var part in target.Split('/'))
            {
                if (part == "..")
                {
                    obj = ((HostObject) obj).GetParent();
                }
                else
                {
                    obj = obj.IGet(part).ToObject();
                }
            }

            return Value.FromObject(obj);
        }

        internal object LoadAptWindowAndQueryPush(string url)
        {
            throw new System.NotImplementedException();
        }
    }
}
