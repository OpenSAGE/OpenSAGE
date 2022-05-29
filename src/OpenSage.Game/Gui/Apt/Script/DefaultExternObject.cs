using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Dom;
using OpenAS2.Runtime.Library;

namespace OpenSage.Gui.Apt.Script
{
    public class DefaultExternObject : HostObject
    {
        private Game _game;

        public DefaultExternObject(VirtualMachine vm, Game game) : base(vm, null, null, false) // TODO check if really is false
        {
            _game = game;

            // Initialize engine variables
            // Mostly no idea what those mean, but they are all booleans

            _properties["InGame"] = PropertyDescriptor.A((_, _, _) => ESCallable.Return(Value.FromBoolean(_game.InGame)), null, true, false);

            foreach (var p in new string[] { "InBetaDemo", "InDreamMachineDemo", "PalantirMinLOD", "MinLOD" })
                _properties[p] = PropertyDescriptor.D(Value.FromBoolean(false), false, true, false);

            foreach (var p in new string[] { "DoTrace" })
                _properties[p] = PropertyDescriptor.D(Value.FromBoolean(true), false, true, false);

            // From RA3
            _properties["gPlatform"] = PropertyDescriptor.D(Value.FromString("PC"), false, true, false);
        }

        public override HostObject GetParent()
        {
            throw new NotImplementedException();
        }
    }
}
