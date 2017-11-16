using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyValues
    {
        internal static VeterancyValues Parse(IniParser parser)
        {
            var values = new List<int>();

            IniToken? token;
            while ((token = parser.GetNextTokenOptional()) != null)
            {
                values.Add(parser.ScanInteger(token.Value));
            }

            return new VeterancyValues
            {
                Values = values.ToArray()
            };
        }

        public int[] Values { get; private set; }
    }
}
