using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenAS2.Base
{
    public static class Utils
    {
        public static string ToCodingForm(this string s)
        {
            return JsonConvert.SerializeObject(s);
        }
    }
}
