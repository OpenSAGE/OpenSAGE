using System;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public abstract class ScriptContent<TDerived, TContentType> : Asset
        where TDerived : ScriptContent<TDerived, TContentType>, new()
        where TContentType : struct
    {
        public TContentType ContentType { get; private set; }
        public string InternalName { get; private set; }

        public ScriptArgument[] Arguments { get; private set; }

        public static TDerived Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                // TODO: Remove this error handling code once we have all content types accounted for.
                TContentType contentType;
                var hasError = false;
                string errorMessage = null;
                try
                {
                    contentType = reader.ReadUInt32AsEnum<TContentType>();
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    contentType = default(TContentType);
                    hasError = true;
                }

                var unknown = reader.ReadByte();
                if (unknown != 3)
                {
                    throw new InvalidDataException();
                }

                var internalNameIndex = reader.ReadUInt24();
                var internalName = context.GetAssetName(internalNameIndex);

                if (hasError)
                {
                    File.AppendAllLines("MissingEnumValues.txt", new[] { $"Missing enum value for script content {internalName}. {errorMessage}" });
                }

                var numArguments = reader.ReadUInt32();
                var arguments = new ScriptArgument[numArguments];

                for (var i = 0; i < numArguments; i++)
                {
                    arguments[i] = ScriptArgument.Parse(reader);
                }

                return new TDerived
                {
                    ContentType = contentType,
                    InternalName = internalName,
                    Arguments = arguments
                };
            });
        }
    }
}
