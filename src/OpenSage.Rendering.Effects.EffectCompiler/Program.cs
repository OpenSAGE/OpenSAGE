using System;
using System.IO;

namespace OpenSage.Rendering.Effects.EffectCompiler;

internal static class Program
{
    public static void Main(string[] args)
    {
        var result = EffectCompiler.Compile(
            args[0],
            Veldrid.GraphicsBackend.Direct3D11);

        if (result.Successful)
        {
            if (args.Length > 1)
            {
                using var effectStream = File.OpenWrite(args[1]);
                using var effectWriter = new BinaryWriter(effectStream);

                result.EffectContent.WriteTo(effectWriter);
            }
        }
        else
        {
            foreach (var message in result.Messages)
            {
                Console.WriteLine(message);
            }
        }
    }
}
