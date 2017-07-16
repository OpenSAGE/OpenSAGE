using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public abstract class ScriptContent<TDerived, TContentType> : Asset
        where TDerived : ScriptContent<TDerived, TContentType>, new()
        where TContentType : struct
    {
        public TContentType ContentType { get; private set; }
        public byte MagicValue { get; private set; }
        public string InternalName { get; private set; }

        public ScriptArgument[] Arguments { get; private set; }

        internal static TDerived Parse(BinaryReader reader, MapParseContext context, ushort minimumVersionThatHasInternalName)
        {
            return ParseAsset(reader, context, version =>
            {
                var contentType = reader.ReadUInt32AsEnum<TContentType>();

                byte magicValue = 0;
                string internalName = null;
                if (version >= minimumVersionThatHasInternalName)
                {
                    magicValue = reader.ReadByte();
                    if (magicValue != 3)
                    {
                        throw new InvalidDataException();
                    }

                    var internalNameIndex = reader.ReadUInt24();
                    internalName = context.GetAssetName(internalNameIndex);
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
                    MagicValue = magicValue,
                    InternalName = internalName,
                    Arguments = arguments
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames, ushort minimumVersionThatHasInternalName)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) (object) ContentType);

                if (Version >= minimumVersionThatHasInternalName)
                {
                    writer.Write(MagicValue);

                    writer.WriteUInt24(assetNames.GetOrCreateAssetIndex(InternalName));
                }

                writer.Write((uint) Arguments.Length);

                foreach (var argument in Arguments)
                {
                    argument.WriteTo(writer);
                }
            });
        }
    }
}
