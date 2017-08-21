using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class UnitSpecificAssets
    {
        internal static UnitSpecificAssets Parse(IniParser parser)
        {
            parser.NextToken(IniTokenType.EndOfLine);

            var result = new UnitSpecificAssets();

            while (parser.Current.TokenType == IniTokenType.Identifier)
            {
                if (parser.Current.TokenType == IniTokenType.Identifier && parser.Current.StringValue.ToUpper() == "END")
                {
                    parser.NextToken();
                    break;
                }
                else
                {
                    var fieldName = parser.Current.StringValue;

                    parser.NextToken();
                    parser.NextToken(IniTokenType.Equals);

                    result.Assets[fieldName] = parser.ParseAssetReference();

                    parser.NextToken(IniTokenType.EndOfLine);
                }
            }

            return result;
        }

        // These keys will eventually mean something to some code, as noted in FactionUnit.ini:32029.
        public Dictionary<string, string> Assets { get; } = new Dictionary<string, string>();
    }
}
