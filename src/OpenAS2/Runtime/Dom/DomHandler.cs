using System.IO;
using System.Linq;
using OpenAS2.Base;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime.Dom
{
    /// <summary>
    /// DOM handler
    /// </summary>
    public abstract class DomHandler
    {



        public virtual ESObject CreateGlobalObject(VirtualMachine vm)
        {
            return new ESObject(vm);
        }

        public virtual bool TryHandle(ExecutionContext context, RawInstruction inst)
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

    }
}
