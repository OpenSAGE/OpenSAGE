using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object.Helpers
{
    // I found, in .sav files, modules called ModuleTag_SMCHelper.
    // Searching in the RA3 SDK, I found ObjectSMCHelper, with the comment
    // SMC = Special Model Condition, noting that this relates to timed model conditions.
    // In RA3 XSDs, ObjectSMCHelperModuleData inherits from ObjectHelperModuleData,
    // so that's the hierarchy I've used here.
    internal sealed class ObjectSpecialModelConditionHelper : ObjectHelperModule
    {
        // TODO

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }
}
