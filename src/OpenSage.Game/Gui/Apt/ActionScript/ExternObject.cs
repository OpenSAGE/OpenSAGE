using System;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class ExternObject : ObjectContext
    {
        /// <summary>
        /// Get an engine variable
        /// </summary>
        /// <param name="name">Name of the engine variable</param>
        /// <returns></returns>
        public override Value GetMember(string name)
        {
            switch (name)
            {
                case "InGame":
                    return Value.FromBoolean(false);
                case "InBetaDemo":
                    return Value.FromBoolean(false);
                case "InDreamMachineDemo":
                    return Value.FromBoolean(false);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
