using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

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
            while ((line = parser.ParseString()) != "EndScript")
            {
                result.Commands.Add(line);
                parser.GoToNextLine();
            }
            return result;
        }

        public List<string> Commands { get; private set; } = new List<string>();
    }
}
