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
            //Mostly no idea what those mean, but they are all booleans
            switch (name)
            {
                case "InGame":
                    return Value.FromBoolean(false);
                case "InBetaDemo":
                    return Value.FromBoolean(false);
                case "InDreamMachineDemo":
                    return Value.FromBoolean(false);
                case "PalantirMinLOD":
                    return Value.FromBoolean(false);
                case "MinLOD":
                    return Value.FromBoolean(false);
                case "DoTrace":
                    return Value.FromBoolean(true);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
