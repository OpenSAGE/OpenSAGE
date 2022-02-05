using OpenAS2.Base;
using OpenAS2.FlashDom.HostObjects;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Dom;
using OpenAS2.FlashDom.Script;
using System;

namespace OpenAS2.FlashDom
{
    public class TraceEventArgs : EventArgs
    {
        public string Trace { get; private set; }

        public TraceEventArgs(string trace)
        {
            Trace = trace;
        }
    }

    public class FlashDomHandler : DomHandler
    {
        public delegate void CommandHandler(ExecutionContext ec, string cmd, string target);
        public delegate SpriteItem MovieHandler(ExecutionContext ec, string url, SpriteItem parent);

        public CommandHandler cmdHandler;
        public MovieHandler movieHandler;
        public event EventHandler TraceCalled = (_, _) => { };

        public static ESObject? GetTarget(ExecutionContext context, string target)
        {
            if (target.Length == 0)
                return context.This;

            if (target.StartsWith('/'))
            {
                var obj = context.Root;
                if (obj == null)
                    return null;
                foreach (var part in target.Split('/'))
                {
                    if (obj == null)
                        return null;
                    if (part == "..")
                    {
                        obj = obj!.GetParent(context);
                    }
                    else
                    {
                        // todo search in movie clip
                        throw new NotImplementedException();
                    }
                }
                return obj;
            }
            else
                throw new NotImplementedException();
        }

        public void HandleUrl(ExecutionContext context, string url, string target)
        {
            Logger.Debug($"[URL] URL: {url} Target: {target}");

            if (url.StartsWith("FSCommand:"))
            {
                var command = url.Replace("FSCommand:", "");

                cmdHandler(context, command, target);
            }
            else
            {
                if (context.This is StageObject so)
                {
                    var targetObject = GetTarget(context, target);
                    var so2 = targetObject as StageObject;
                    if (so2 == null || !(so2.Item is SpriteItem))
                    {
                        Logger.Error($"[URL] Target must be a sprite, but got {target}!");
                    }
                    else
                    {
                        var targetSprite = so2!.Item as SpriteItem;
                        var aptFile = movieHandler(context, url, targetSprite);
                        var oldName = targetSprite.Name;

                        throw new NotImplementedException();
                        // targetSprite.Create(aptFile.Movie, targetSprite.Context, targetSprite.Parent);
                        targetSprite.Name = oldName;
                    }
                }
            }
        }

        public override bool TryHandle(ExecutionContext context, RawInstruction inst)
        {
            var root = context.Root as StageObject;
            SpriteItem? item = root != null ? root.Item as SpriteItem : null;
            var errInfo = $"{inst.Type} called on a non-stage object.";
            switch (inst.Type)
            {
                case InstructionType.NextFrame:
                    if (item != null)
                        item.NextFrame();
                    else
                        Logger.Warn(errInfo);
                    break;
                case InstructionType.PrevFrame:
                    return false; // TODO
                case InstructionType.Play:
                    if (item != null)
                        item.Play();
                    else
                        Logger.Warn(errInfo);
                    break;
                case InstructionType.Stop:
                    if (item != null)
                        item.Stop();
                    else
                        Logger.Warn(errInfo);
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
                    HandleUrl(context, inst.Parameters[0].String, inst.Parameters[1].String);
                    break;
                case InstructionType.GetURL2:
                    context.EnqueueResultCallback(context.Pop().ToString(context).AddRecallCode(res =>
                    {
                        return context.Pop().ToString(context).AddRecallCode(res2 =>
                        {
                            HandleUrl(context, res.Value.ToString(), res2.Value.ToString());
                            return null;
                        });
                    }));
                    break;


                case InstructionType.GetTime:
                    break;


                case InstructionType.Trace:
                    context.EnqueueResultCallback(context.Pop().ToString(context).AddRecallCode(rc => {
                        TraceCalled.Invoke(context, new TraceEventArgs(rc.Value.ToString()));
                        return null;
                        }));
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

    }
}
