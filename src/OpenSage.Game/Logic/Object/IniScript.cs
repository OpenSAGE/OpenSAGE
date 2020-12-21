using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public static class IniScript
    {
        internal static string Parse(IniParser parser)
        {
            var result = "";
            string line;
            parser.GoToNextLine();
            while (!(line = parser.ParseLine()).Contains("EndScript"))
            {
                result += line;
                parser.GoToNextLine();
            }
            return result;
        }
    }
}
