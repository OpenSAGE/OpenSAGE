using System.Linq;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    internal sealed class ASColor : BuiltinClass
    {
        private ColorRgba _color = ColorRgba.White;

        public ASColor() : base()
        {
            //list of builtin functions
            _builtinFunctions.Add("setRGB", setRGB);
        }

        private Value setRGB(Value[] args)
        {
            _color = ColorRgba.FromHex(_color, args.First().ToString());
            return Value.Undefined();
        }
    }
}
