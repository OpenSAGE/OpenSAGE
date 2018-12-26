using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenSage.Data.Utilities
{
    internal static class ParseUtility
    {
        private static readonly Regex FloatRegex = new Regex("^([-+]?[0-9]*\\.?[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex IntegerRegex = new Regex("^([-+]?[0-9]*)", RegexOptions.Compiled);

        public static float ParseFloat(string s)
        {
            s = ExtractFloat(s);
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public static bool TryParseFloat(string s, out float result)
        {
            s = ExtractFloat(s);
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static string ExtractFloat(string s)
        {
            var match = FloatRegex.Match(s);
            return match.Success
                ? match.Groups[1].Value
                : string.Empty;
        }

        public static string ToInvariant(float number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }

        public static int ParseInteger(string s)
        {
            s = ExtractInteger(s);
            return Convert.ToInt32(s);
        }

        private static string ExtractInteger(string s)
        {
            var match = IntegerRegex.Match(s);
            return match.Success
                ? match.Groups[1].Value
                : string.Empty;
        }
    }
}
