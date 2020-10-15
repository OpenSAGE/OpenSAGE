﻿namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class ExternObject : ObjectContext
    {
        private VM _vm;

        public ExternObject(VM vm)
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
            return _vm.VariableHandler(name);
        }
    }
}
