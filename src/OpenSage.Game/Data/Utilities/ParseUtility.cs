using System.Globalization;

namespace OpenSage.Data.Utilities
{
    internal static class ParseUtility
    {
        public static float ParseFloat(string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public static bool TryParseFloat(string s, out float result)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static string ToInvariant(float number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}
