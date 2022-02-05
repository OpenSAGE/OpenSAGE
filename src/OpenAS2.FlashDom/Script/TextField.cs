using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.FlashDom.HostObjects;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Library;

namespace OpenAS2.FlashDom.Script
{
    public class TextField : StageObject
    {
        public static new Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            ["textColor"] = () => PropertyDescriptor.A(
                (_, tv, _) => Value.FromString(((Text)((StageObject)tv).Item.Character).Color.ToHex()),
                (ec, tv, args) =>
                {
                    if (HasArgs(args))
                    {
                        var ctx = (StageObject)tv;
                        var hexStrRes = args[0].ToString(ec).AddRecallCode(res =>
                        {
                            var hexStr = res.Value.ToString();
                            var hexColor = Convert.ToInt32(hexStr, 16);

                            var b = (hexColor & 0xFF) / 255.0f;
                            var g = ((hexColor & 0xFF00) >> 8) / 255.0f;
                            var r = ((hexColor & 0xFF0000) >> 16) / 255.0f;

                            var transform = ctx.Item.Transform;
                            ctx.Item.Transform =
                                transform.WithColorTransform(transform.ColorTransform.WithRGB(r, g, b));
                            return ESCallable.Normal(Value.Undefined());
                        });
                        return hexStrRes;
                        
                    }
                    return ESCallable.Normal(Value.Undefined());
                },
                true, false),
        };

        public static new readonly Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        public static new readonly Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {

        };

        public static new readonly Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        public TextField(RenderItem item, VirtualMachine? vm = null) : base(item, vm, "TextField") { }
    }

}
