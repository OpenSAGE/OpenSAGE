using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Library;
using ValueType = OpenAS2.Runtime.ValueType;

namespace OpenSage.Gui.Apt.Script
{


    public class MovieClip : StageObject
    {
        public static new Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>(StageObject.PropertiesDefined)
        {
            ["_currentframe"] = () => PropertyDescriptor.A(
                (ec, tv, _) => ESCallable.Return(Value.FromInteger(((SpriteItem)((StageObject)tv).Item).CurrentFrame)),
                (ec, _, _) => ESCallable.Throw(ec.ConstrutError("Error", "Not Implemented")),
                false, false),
        };

        public static new readonly Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>(StageObject.MethodsDefined)
        {
            ["gotoAndPlay"] = (actx, tv, args) =>
            {
                ((MovieClip)tv).GotoAndPlay(actx, args);
                return ESCallable.Normal(Value.Undefined());
            },
            ["gotoAndStop"] = (actx, tv, args) =>
            {
                ((MovieClip)tv).GotoAndStop(args);
                return ESCallable.Normal(Value.Undefined());
            },
            ["stop"] = (actx, tv, args) =>
            {
                ((MovieClip)tv).Stop();
                return ESCallable.Normal(Value.Undefined());
            },
            ["loadMovie"] = (actx, tv, args) =>
            {
                ((MovieClip)tv).LoadMovie(actx, args);
                return ESCallable.Normal(Value.Undefined());
            },
            ["attachMovie"] = (actx, tv, args) =>
            {
                var m = ((MovieClip)tv).AttachMovie(actx, args);
                return ESCallable.Return(Value.FromObject(m));
            },
        };

        public static new readonly Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>(StageObject.StaticPropertiesDefined)
        {

        };

        public static new readonly Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>(StageObject.StaticMethodsDefined)
        {

        };


        public MovieClip(SpriteItem item, VirtualMachine? vm = null) : base(item, vm, "MovieClip") { }

        public void GotoAndPlay(ExecutionContext actx, IList<Value> args)
        {
            if (Item is SpriteItem si)
            {
                var dest = actx.ResolveRegister(args.First().ToInteger());

                if (dest.Type == ValueType.String)
                {
                    si.Goto(dest.ToString());
                }
                else if (dest.Type == ValueType.Integer)
                {
                    si.GotoFrame(dest.ToInteger() - 1);
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or frame numbers");
                }

                si.Play();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void GotoAndStop(IList<Value> args)
        {
            if (Item is SpriteItem si)
            {
                var dest = args.First();

                if (dest.Type == ValueType.String)
                {
                    si.Goto(dest.ToString());
                }
                else if (dest.Type == ValueType.Integer)
                {
                    si.GotoFrame(dest.ToInteger() - 1);
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or frame numbers");
                }

                si.Stop(true);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Stop()
        {
            if (Item is SpriteItem si)
            {
                si.Stop();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void LoadMovie(ExecutionContext context, IList<Value> args)
        {
            var url = Path.ChangeExtension(args[0].ToString(), ".apt");
            var dom = (AptDomHandler)context.Dom;
            // TODO Change to MovieHandler
            var window = dom.Window.Manager.Game.LoadAptWindow(url);

            dom.Window.Manager.QueryPush(window);
        }

        public MovieClip AttachMovie(ExecutionContext context, IList<Value> args)
        {
            var url = Path.ChangeExtension(args[0].ToString(), ".apt");
            var name = args[1].ToString();
            var depth = args[2].ToInteger();

            throw new NotImplementedException();
        }

    }
}
