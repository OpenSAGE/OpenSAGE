using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class IniScript
    {
        internal static IniScript Parse(IniParser parser)
        {
            var result = new IniScript();
            string line;
            parser.GoToNextLine();
            while (!(line = parser.ParseLine()).Contains("EndScript"))
            {
                result.Code += line/* + "\r\n"*/;
                parser.GoToNextLine();
            }
            return result;
        }

        public string Code { get; private set; }
    }
}
