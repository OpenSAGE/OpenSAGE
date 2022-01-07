using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Runtime
{
    public static class Logger
    {
        public static void Warn (string info)
        {
            Console.WriteLine(info);
        }
        public static void Error(string info)
        {
            Console.WriteLine(info);
        }
        public static void Debug(string info)
        {
            Console.WriteLine(info);
        }
    }
}
