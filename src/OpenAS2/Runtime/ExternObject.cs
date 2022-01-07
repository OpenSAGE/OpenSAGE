using System;

namespace OpenAS2.Runtime
{
    public sealed class ExternObject : ASObject
    {
        private VirtualMachine _vm;

        public ExternObject(VirtualMachine vm): base(vm)
        {
            _vm = vm;
        }

        /// <summary>
        /// Get an engine variable
        /// </summary>
        /// <param name="name">Name of the engine variable</param>
        /// <returns></returns>
        public override Value GetMember(string name)
        {
            return _vm.Dom.VariableHandler(name);
        }

        public override void SetMember(string name, Value val)
        {
            throw new NotImplementedException();
        }
    }
}
