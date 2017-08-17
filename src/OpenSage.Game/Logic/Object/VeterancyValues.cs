using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyValues
    {
        internal static VeterancyValues Parse(IniParser parser)
        {
            var values = new List<int>();

            while (parser.CurrentTokenType == IniTokenType.IntegerLiteral)
            {
                values.Add(parser.ParseInteger());
            }

            return new VeterancyValues
            {
                Values = values.ToArray()
            };
        }

        public int[] Values { get; private set; }
    }
}
