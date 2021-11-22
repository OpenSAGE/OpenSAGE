using System;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using NLog.Targets;

using Veldrid;

namespace OpenSage.Launcher
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Target.Register<Core.InternalLogger>("OpenSage");
            GameWrapper gameBuilder = new GameWrapper();
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => gameBuilder.Initialize(opts));
        }
    }
}
