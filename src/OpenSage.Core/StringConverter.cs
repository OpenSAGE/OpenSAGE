using System.Text;

namespace OpenSage.Core
{
    /// <summary>
    /// Convert a printf formatted string to a C# formatted string
    /// </summary>
    public class StringConverter
    {
        //TODO: implement this properly
        public static string FromPrintf(string printf)
        {
            var builder = new StringBuilder();
            int index = 0;
            int state = 0;

            foreach (var c in printf)
            {
                if (state == 0)
                {
                    if (c == '%')
                    {
                        state = 1;
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
                else if (state == 1)
                {
                    if (c == 'f' || c == 'i')
                    {
                        builder.Append("{" + index + "}");
                        index++;
                        state = 0;
                    }
                    // Percentage was escaped
                    if (c == '%')
                    {
                        builder.Append(c);
                        state = 0;
                    }
                }
            }

            return builder.ToString();
        }
    }
}
