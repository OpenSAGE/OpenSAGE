using System.Linq;
using System;
using System.Collections.Generic;
using OpenAS2.Runtime.Dom;
using OpenAS2.Runtime;
using OpenSage.Mathematics;
using OpenAS2.Runtime.Library;

namespace OpenAS2.FlashDom.Script
{
    public sealed class ASColor : ASObject
    {
        public static new Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {

        };

        public static new readonly Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            ["getRGB"] = (vm, tv, args) => {
                     var ans = ((ASColor)tv).getRGB();
                     return ESCallable.Return(ans);
                },
            ["setRGB"] = (vm, tv, args) => {
                    ((ASColor)tv).setRGB(args);
                    return ESCallable.Normal(Value.Undefined());
                },
        };

        public static new readonly Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {

        };

        public static new readonly Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        private ColorRgba _color = ColorRgba.White;

        public ASColor(VirtualMachine vm) : base(vm, "Color", "Object", true)
        {

        }

        public Value getRGB()
        {
            return Value.FromInteger(_color.ToIntegerRGB());
        }
        public void setRGB(IList<Value>? args)
        {
            if (args != null)
                _color = ColorRgba.FromHex(_color, args.First().ToString());
        }
    }
}
